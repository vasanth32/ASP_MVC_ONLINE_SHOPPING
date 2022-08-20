using OnlineShopping.DAL;
using OnlineShopping.Filters;
using OnlineShopping.Models;
using OnlineShopping.Repository;
using OnlineShopping.Utility;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace OnlineShopping.Controllers
{
    [FrontPageActionFilter]
    public class AccountController : Controller
    {
        #region Other class references...
        // Instance on Unit of Work
        public GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();

        #endregion

        #region Member Login ...         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                string EncryptedPassword = EncryptDecrypt.Encrypt(model.Password, true);
                var user = _unitOfWork.GetRepositoryInstance<Tbl_Members>().GetFirstOrDefaultByParameter(i => i.EmailId == model.UserEmailId && i.Password == EncryptedPassword && i.IsDelete == false);
                if (user != null && user.IsActive == true)
                {
                    Session["MemberId"] = user.MemberId;
                    Response.Cookies["MemberName"].Value = user.FirstName;
                    var roles = _unitOfWork.GetRepositoryInstance<Tbl_MemberRole>().GetFirstOrDefaultByParameter(i => i.MemberId == user.MemberId);
                    if (roles != null && roles.RoleId != 1)
                    {
                        Response.Cookies["MemberRole"].Value = _unitOfWork.GetRepositoryInstance<Tbl_Roles>().GetFirstOrDefaultByParameter(i => i.RoleId == roles.RoleId).RoleName;
                    }
                    else
                    {
                        ModelState.AddModelError("Password", "Invalid username or password");
                        return View(model);
                    }
                    if (model.RememberMe)
                    {
                        Response.Cookies["RememberMe_UserEmailId"].Value = model.UserEmailId; Response.Cookies["RememberMe_Password"].Value = model.Password;
                    }
                    else
                    {
                        Response.Cookies["RememberMe_UserEmailId"].Expires = DateTime.Now.AddDays(-1); Response.Cookies["RememberMe_Password"].Expires = DateTime.Now.AddDays(-1);
                    }
                    ViewBag.redirectUrl = (!string.IsNullOrEmpty(returnUrl) ? HttpUtility.HtmlDecode(returnUrl) : "/");
                }
                else
                {
                    if (user != null && user.IsActive == false) ModelState.AddModelError("Password", "Your account in not verified");
                    else ModelState.AddModelError("Password", "Invalid username or password");
                }
            }
            return PartialView("_Login", model);
        }
        #endregion


        #region Member Registration ...         
        [AllowAnonymous]
        public ActionResult Register()
        {
            RegisterViewModel model = new RegisterViewModel();
            model.UserType = 2;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Register(RegisterViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {                 // Adding Member                 
                Tbl_Members mem = new Tbl_Members();
                mem.FirstName = model.FirstName;
                mem.LastName = model.LastName;
                mem.EmailId = model.UserEmailId;
                mem.CreatedOn = DateTime.Now;
                mem.ModifiedOn = DateTime.Now;
                mem.Password = EncryptDecrypt.Encrypt(model.Password, true);
                mem.IsActive = true;
                mem.IsDelete = false;
                _unitOfWork.GetRepositoryInstance<Tbl_Members>().Add(mem);
                // Adding Member Role                 
                Tbl_MemberRole mem_Role = new Tbl_MemberRole();
                mem_Role.MemberId = mem.MemberId;
                mem_Role.RoleId = 2;
                _unitOfWork.GetRepositoryInstance<Tbl_MemberRole>().Add(mem_Role);

                TempData["VerificationLinlMsg"] = "You are registered successfully.";
                Session["MemberId"] = mem.MemberId;
                Response.Cookies["MemberName"].Value = mem.FirstName;
                Response.Cookies["MemberRole"].Value = "User";
                return RedirectToAction("Index", "Home");
            }
            return View("Register", model);
        }


        public JsonResult CheckEmailExist(string UserEmailId)
        {
            int LoginMemberId = Convert.ToInt32(Session["MemberId"]);
            var EmailExist = _unitOfWork.GetRepositoryInstance<Tbl_Members>().GetFirstOrDefaultByParameter(i => i.MemberId != LoginMemberId && i.EmailId == UserEmailId && i.IsDelete == false);
            return EmailExist == null ? Json(true, JsonRequestBehavior.AllowGet) : Json(false, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Logout ...
        public ActionResult LogOut()
        {
            Session["MemberId"] = null;
            if (Request.Cookies["MemberRole"] != null)
                Response.Cookies["MemberRole"].Expires = DateTime.Now.AddDays(-1);
            return RedirectToAction("Index", "Home");
        }
        #endregion


        #region ForgotPassword ...
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel fpm)
        {
            if (_unitOfWork.GetRepositoryInstance<Tbl_Members>().GetFirstOrDefaultByParameter(i => i.EmailId == fpm.EmailId && i.IsActive == true && i.IsDelete == false) != null)
            {
                string body = string.Empty;
                string subject = string.Empty;
                using (var sr = new StreamReader(Server.MapPath("~/EmailTemplates/") + "ForgotPassword.html"))
                {
                    body = sr.ReadToEnd();
                    subject = "Online Shopping : Reset Password";
                }
                body = body.Replace("_ResetPasswordUrl", System.Configuration.ConfigurationManager.AppSettings["ApplicationRootUrl"] + "/Account/ResetPassword?EmailId=" + EncryptDecrypt.Encrypt(fpm.EmailId, true));
                EmailNotification.SendMail(fpm.EmailId, subject, body);
                ViewBag.SendEmailMessage = "Password reset link is sent at your email address. Please check your email";
            }
            else
                ModelState.AddModelError("EmailId", "Email Not Registered");
            return View(fpm);

        }
        #endregion

        #region Reset Password ...
        [HttpGet]
        public ActionResult ResetPassword(string EmailId)
        {
            ResetPasswordViewModel rpm = new ResetPasswordViewModel();
            if (EmailId != null)
            {
                try
                {
                    ModelState.Clear();
                    rpm.EmailId = EncryptDecrypt.Decrypt(EmailId, true);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Invalid email address");
                }
            }
            else
                ModelState.AddModelError("", "Invalid email address");
            return View(rpm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult ResetPassword(ResetPasswordViewModel rpm)
        {
            var ExistingDetails = _unitOfWork.GetRepositoryInstance<Tbl_Members>().GetFirstOrDefaultByParameter(i => i.EmailId == rpm.EmailId && i.IsActive == true && i.IsDelete == false);
            if (ExistingDetails != null)
            {
                ExistingDetails.Password = EncryptDecrypt.Encrypt(rpm.NewPassword, true);
                ExistingDetails.ModifiedOn = DateTime.UtcNow;
                _unitOfWork.GetRepositoryInstance<Tbl_Members>().Update(ExistingDetails);
                _unitOfWork.SaveChanges();
                ViewBag.PasswordChangeMsg = "Password Changed Successfully";
            }
            else
                ModelState.AddModelError("", "Invalid Email Address");
            return View(rpm);
        }
        #endregion   

        #region Unauthorize View to Page...
        public ActionResult UnauthorizeViewToPage()
        {
            return Redirect("/");
        }
        #endregion


    }
}
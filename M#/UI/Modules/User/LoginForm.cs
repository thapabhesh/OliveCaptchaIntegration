using MSharp;

namespace Modules
{
    public class LoginForm : FormModule<Domain.User>
    {
        public LoginForm()
        {
            SupportsAdd(false).Using(new[] { "Olive.Security", "BotDetect.Web.Mvc" })
                .SupportsEdit(false)
                .HeaderText("Login")
                .DataSource("await Domain.User.FindByEmail(info.Email)");

            Field(x => x.Email).WatermarkText("Your email");
            Field(x => x.Password).Mandatory().WatermarkText("Your password");
            CustomField().ControlMarkup(@"<div>
	@{var loginCaptcha = Website.CaptchaHelper.GetLoginCaptcha();}
	<captcha mvc-captcha=""loginCaptcha"" />
	<div class=""actions"">
		<input asp-for=""CaptchaCode"" class=""form-control"" placeholder=""Captcha code"" />
	</div>
</div>").VisibleIf("info.ShowCaptcha");

            Button("Login").ValidateAntiForgeryToken(false).CssClass("w-100 btn-login mb-2")
            .OnClick(x =>
            {
                x.RunInTransaction(false);
                x.ShowPleaseWait();

                x.CSharp(@"var mvcCaptcha = Website.CaptchaHelper.GetLoginCaptcha();");
                x.If("info.ShowCaptcha && !mvcCaptcha.Validate(info.CaptchaCode, Request.Param(mvcCaptcha.ValidatingInstanceKey))")
                  .CSharp(@"Notify(""Invalid Captcha"", ""error"");
                            info.ShowCaptcha = await LogonFailure.MustShowCaptcha(info.Email, Request.GetIPAddress());
                            return View(info);");

                x.CSharp("var user = await Domain.User.FindByEmail(info.Email);");
                x.If("user == null || !SecurePassword.Verify(info.Password, user.Password, user.Salt)")
                 .CSharp(@"Notify(""Invalid username or password"", ""error"");
                        info.ShowCaptcha = await LogonFailure.MustShowCaptcha(info.Email, Request.GetIPAddress());
                        return View(info); ");
                x.CSharp("await user.LogOn();");
                x.CSharp("await LogonFailure.Remove(info.Email, Request.GetIPAddress());");
                x.If(CommonCriterion.RequestHas_ReturnUrl).ReturnToPreviousPage();
                x.Go<Login.DispatchPage>();
            });

            Link("Forgot password?").CssClass("text-info").OnClick(x => x.Go<Login.ForgotPasswordPage>());

            ViewModelProperty<bool>("ShowCaptcha").RetainInPost();

            ViewModelProperty<string>("CaptchaCode").NotReadOnly();

            Reference<ContentBlockView>();
        }
    }
}

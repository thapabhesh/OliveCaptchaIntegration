# Olive Captcha Integration
This project explores the [Captcha](https://www.nuget.org/packages/Captcha/) integration with [Olive Framework](https://github.com/Geeksltd/Olive).

The example is based on the training article provided on Captcha website. https://captcha.com/doc/aspnet/samples/csharp/asp.net-mvc-basic-captcha-sample.html.

## Prerequisite
This sample project uses Olive framework for proof of concept. Please follow the installation guidelines if you want to run the sample project and do not already have Msharp installed on your local machine. http://learn.msharp.co.uk/#/Install/README

### Step 1: Install Nuget package
Install Captcha nuget package on your Website project. The version that I am using is version 4.4.0.

### Step 2: Configure the Startup
Open Startup.cs and add the following lines.

```csharp

public override void ConfigureServices(IServiceCollection services)
{
    base.ConfigureServices(services);
    services.AddDatabaseLogger();
    services.AddScheduledTasks();

    if (Environment.IsDevelopment())
        services.AddDevCommands(x => x.AddTempDatabase<SqlServerManager, ReferenceData>());

    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    services.AddMemoryCache();
    services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(20);
    });
}

```

Override `ConfigureRequestHandlers` method with the following piece of code.

```csharp

protected override void ConfigureRequestHandlers(IApplicationBuilder app)
{
    app.UseSession();
    UseStaticFiles(app);
    app.UseRequestLocalization(RequestLocalizationOptions);
            
    app.UseCaptcha(Configuration);
    app.UseMvc();
}

```

### Step 3: Add CaptchaHelper class

```csharp

using BotDetect;
using BotDetect.Web.Mvc;

namespace Website
{
    public static class CaptchaHelper
    {
        public static MvcCaptcha GetLoginCaptcha()
        {
            return new MvcCaptcha("LoginCaptcha")
            {
                UserInputID = "CaptchaCode",
                ImageFormat = ImageFormat.Jpeg,
            };
        }
    }
}

```

### Step 4: Register TagHelper
Open _ViewImports.cshtml and add `@addTagHelper "BotDetect.Web.Mvc.CaptchaTagHelper, BotDetect.Web.Mvc"` at the end of the file.

### Step 5: Link stylesheet
Open Blank.Container.cshtml and add `<link href="@BotDetect.Web.CaptchaUrls.Absolute.LayoutStyleSheetUrl" rel="stylesheet" type="text/css" />` within head section.

### Step 6: View
Add following piece of code to the Login.cshtml just below Password input.

```html

@if (info.ShowCaptcha)
{
<div class="form-group row">
    <label class="control-label">
    </label>
    <div class="group-control">
        <div>
        @{var loginCaptcha = Website.CaptchaHelper.GetLoginCaptcha();}
        <captcha mvc-captcha="loginCaptcha" />
        <div class="actions">
            <input asp-for="CaptchaCode" class="form-control" placeholder="Captcha code" />
        </div>
        </div>
    </div>
</div>
}

```

### Step 7: Controller
Update your Login controller action to following.

```csharp

[HttpPost("LoginForm/Login")]
public async Task<ActionResult> Login(vm.LoginForm info)
{
    var mvcCaptcha = Website.CaptchaHelper.GetLoginCaptcha();
            
    if (info.ShowCaptcha && !mvcCaptcha.Validate(info.CaptchaCode, Request.Param(mvcCaptcha.ValidatingInstanceKey)))
    {
        Notify("Invalid Captcha", "error");
        info.ShowCaptcha = await LogonFailure.MustShowCaptcha(info.Email, Request.GetIPAddress());
        return View(info);
    }
            
    var user = await Domain.User.FindByEmail(info.Email);
            
    if ((user == null || !SecurePassword.Verify(info.Password, user.Password, user.Salt)))
    {
        Notify("Invalid username or password", "error");
        info.ShowCaptcha = await LogonFailure.MustShowCaptcha(info.Email, Request.GetIPAddress());
        return View(info);
    }
            
    await user.LogOn();
            
    await LogonFailure.Remove(info.Email, Request.GetIPAddress());
            
    if (Url.ReturnUrl().HasValue())
    {
        return Redirect(Url.ReturnUrl());
    }
            
    return Redirect(Url.Index("LoginDispatch"));
}

```

### Step 8: Image style (Optional)
Captcha plugin will randomize between various image styles. If you want to restrict the styles to limited set of styles then you can add the style configuration on appsettings.json

```json

"BotDetect": {
  "ImageStyle": "Bullets, BlackOverlap, Overlap"
}

```
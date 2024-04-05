(function ($) {
  const emailInput = $("#username");
  const passwordInput = $("#password");
  const loginButton = $("#loginButton");

  /*
  emailInput.on("input", function() {
      const email = emailInput.val().trim();
      const emailRegex = /^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-z]{2,}$/;
      console.log(email);
      if (emailRegex.test(email)) {
          emailInput.css("borderColor", "");
          enableDisableLoginButton();
      } else {
          emailInput.css("borderColor", "#d98c93");
          enableDisableLoginButton();
      }
  });
  */

  passwordInput.on("input", function() {
      const password = passwordInput.val();
      const hasUppercase = /[A-Z]/.test(password);
      const hasLowercase = /[a-z]/.test(password);
      const hasSpecialChar = /[@$!%*?&.]/.test(password);
      const hasNumber = /\d/.test(password);

      if (hasUppercase && hasLowercase && hasSpecialChar && hasNumber && password.length >= 8) {
          passwordInput.css("borderColor", "");
          enableDisableLoginButton();
      } else {
          passwordInput.css("borderColor", "#d98c93");
          enableDisableLoginButton();
      }
  });

  function enableDisableLoginButton() {
      const email = emailInput.val();
      const password = passwordInput.val();
      const emailRegex = /^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-z]{2,}$/;
      const hasUppercase = /[A-Z]/.test(password);
      const hasLowercase = /[a-z]/.test(password);
      const hasSpecialChar = /[@$!%*?&.]/.test(password);
      const hasNumber = /\d/.test(password);

      if (emailRegex.test(email) && password && hasUppercase && hasLowercase && hasSpecialChar && hasNumber && password.length >= 8) {
          loginButton.prop("disabled", false);
      } else {
          loginButton.prop("disabled", true);
      }
  }

  // Kiểm tra khi trang web tải lần đầu
  enableDisableLoginButton();

}(jQuery));	

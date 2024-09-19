(function ($) {
  $('#form-Login').on('submit', function (e) {
    if (validateAll() && this.checkValidity()) {
      $('.loading').removeClass('hidden');
    }
    else {
      e.preventDefault();
    }
  });

  const emailInput = $("#username");
  const passwordInput = $("#password");
  const loginButton = $("#loginButton");

  emailInput.on("input", validateEmail);
  //passwordInput.on("input", validatePasswordFormat);

  function validateEmail() {
    const email = emailInput.val().trim();
    const emailPattern = /^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$/;
    if (email !== "" && emailPattern.test(email)) {
      emailInput.removeClass("content-border-danger");
      return true;
    } else {
      emailInput.addClass("content-border-danger");
      return false;
    }
  }

  function validatePasswordFormat() {
    var password = passwordInput.val();
    const conditions = [
      /[A-Z]/.test(password),
      /[a-z]/.test(password),
      /\d/.test(password),
      /[!@#$%^&*(),.?":{}|<>]/.test(password),
      password.length >= 8,
    ];
    const totalConditions = conditions.length;
    let correctConditions = conditions.filter(condition => condition).length;
    var result = Math.round((correctConditions / totalConditions) * 100);

    if (result !== 100) {
      passwordInput.addClass("content-border-danger")
    } else {
      passwordInput.removeClass("content-border-danger");
    }

    return result;
  }

  function validateAll() {
    let isDoneAllValidate = true;

    // Validate each field
    const isValidEmail = validateEmail();
    //const isValidPassword = validatePasswordFormat() === 100;

    if (!isValidEmail) isDoneAllValidate = false;
    //if (!isValidPassword) isDoneAllValidate = false;

    return isDoneAllValidate;
  }

  function enableDisableLoginButton() {
      const email = emailInput.val();
      const password = passwordInput.val();
      //const emailRegex = /^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-z]{2,}$/;
      const hasUppercase = /[A-Z]/.test(password);
      const hasLowercase = /[a-z]/.test(password);
      const hasSpecialChar = /[@$!%*?&.]/.test(password);
      const hasNumber = /\d/.test(password);

      if (/*emailRegex.test(email) &&*/ email && password && hasUppercase && hasLowercase && hasSpecialChar && hasNumber && password.length >= 8) {
          loginButton.prop("disabled", false);
      } else {
          loginButton.prop("disabled", true);
      }
  }

  // Kiểm tra khi trang web tải lần đầu
  //enableDisableLoginButton();

}(jQuery));	

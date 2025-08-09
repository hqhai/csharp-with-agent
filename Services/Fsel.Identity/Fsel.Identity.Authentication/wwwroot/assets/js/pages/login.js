(function ($) {
  $('#form-Login').on('submit', function (e) {
    if (validateAll() && this.checkValidity()) {
      $('.loading').removeClass('hidden');
    }
    else {
      e.preventDefault();
    }
  });

  const identityInput = $("#username");
  const passwordInput = $("#password");
  const loginButton = $("#loginButton");

  identityInput.on("input", validateEmail);
  function validateEmail() {
    const identity = identityInput.val();
    const emailPattern = /^(?=.{1,64}@)(?=.{1,255}$)[a-zA-Z0-9!#$%&'*+/=?^_{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?$/;
    const phonePattern = /^\+?\d{7,15}$/;

    if (identity !== "") {
      if (phonePattern.test(identity)) {
        identityInput.removeClass("content-border-danger");
        return true;
      }
      if (emailPattern.test(identity)) {
        identityInput.removeClass("content-border-danger");
        return true;
      }
    }
    identityInput.addClass("content-border-danger");
    return false;
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

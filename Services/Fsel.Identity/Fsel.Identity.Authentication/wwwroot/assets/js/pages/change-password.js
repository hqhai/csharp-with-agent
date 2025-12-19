(function ($) {
  $('#form-ForgotPassword').on('submit', function (e) {
    if (validateAll() && this.checkValidity()) {
      $('.loading').removeClass('hidden');
    }
    else {
      e.preventDefault();
    }
  });

  const $newPasswordInput = $("#Password");
  const $passwordMeter = $("#passwordMeter");
  const $meterText = $("#meter-text");
  const $ruleList = $("#rule-list");
  const $ruleItems = $(".rule-item");
  const $passLengthItem = $("#pass-length");
  const $passNumberItem = $("#pass-number");
  const $passUpCaseItem = $("#pass-Up-case");
  const $passSymboItem = $("#pass-symbo");


  function validatePasswordFormat(password) {
    var password = $newPasswordInput.val();
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
      $newPasswordInput.addClass("content-border-danger")
    } else {
      $newPasswordInput.removeClass("content-border-danger");
    }

    return result;
  }

  function validateNewPassword() {
    const newPassword = $newPasswordInput.val();

    const isValidNewPassword = validatePasswordFormat(newPassword);
    $passwordMeter.css("width", `${isValidNewPassword}%`);
    if (isValidNewPassword > 80) {
      $meterText.text($meterText.attr("text-strong"));
      $passwordMeter.css("background-color", "#3E8E41");
    } else if (isValidNewPassword > 40) {
      $meterText.text($meterText.attr("text-good"));
      $passwordMeter.css("background-color", "#EC9213");
    } else {
      $meterText.text($meterText.attr("text-weak"));
      $passwordMeter.css("background-color", "#C0404C");
    }

    if (newPassword) {
      $ruleList.css("display", "grid");
    } else {
      $meterText.text("");
    }

    if (isValidNewPassword !== 100) {
      $newPasswordInput.addClass("content-border-danger");
      return false;
    } else {
      $newPasswordInput.removeClass("content-border-danger");
    }

    // Update password rules
    updatePasswordRules(newPassword);
    updateContinueButtonStatePass();
    return true;
  }

  function updatePasswordRules(password) {
    const passLengthItem = $passLengthItem;
    const passNumberItem = $passNumberItem;
    const passUpCaseItem = $passUpCaseItem;
    const passSymboItem = $passSymboItem;

    // Length rule
    if (password.length >= 8) {
      passLengthItem.css("color", "var(--white-color)");
      passLengthItem.find("img").attr("src", "/assets/icons/check-icon.svg");
    } else {
      passLengthItem.css("color", "var(--disable-text-color)");
      passLengthItem.find("img").attr("src", "/assets/icons/close-icon.svg");
    }

    // Number rule
    if (/\d/.test(password)) {
      passNumberItem.css("color", "var(--white-color)");
      passNumberItem.find("img").attr("src", "/assets/icons/check-icon.svg");
    } else {
      passNumberItem.css("color", "var(--disable-text-color)");
      passNumberItem.find("img").attr("src", "/assets/icons/close-icon.svg");
    }

    // Uppercase rule
    if (/(?=.*[a-z])(?=.*[A-Z])/.test(password)) {
      passUpCaseItem.css("color", "var(--white-color)");
      passUpCaseItem.find("img").attr("src", "/assets/icons/check-icon.svg");
    } else {
      passUpCaseItem.css("color", "var(--disable-text-color)");
      passUpCaseItem.find("img").attr("src", "/assets/icons/close-icon.svg");
    }

    // Symbol rule
    if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) {
      passSymboItem.css("color", "var(--white-color)");
      passSymboItem.find("img").attr("src", "/assets/icons/check-icon.svg");
    } else {
      passSymboItem.css("color", "var(--disable-text-color)");
      passSymboItem.find("img").attr("src", "/assets/icons/close-icon.svg");
    }
  }

  function validateAll() {
    let isDoneAllValidate = true;

    // Validate each field
    const isValidPassword = validateNewPassword();

    if (!isValidPassword) isDoneAllValidate = false;

    return isDoneAllValidate;
  }

  function updateContinueButtonStatePass() {
    const isValidNewPassword = validatePasswordFormat($newPasswordInput.val()) === 100;
    // $continueButton.prop('disabled', !isValidNewPassword);
  }

  $newPasswordInput.on("input", validateAll);

}(jQuery));	

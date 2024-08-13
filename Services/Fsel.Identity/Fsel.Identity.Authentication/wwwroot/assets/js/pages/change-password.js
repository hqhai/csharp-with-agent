(function ($) {
  $('#form-ForgotPassword').on('submit', function (e) {
    if (this.checkValidity()) {
      $('.loading').removeClass('hidden');
    }
    else {
      e.preventDefault();
    }
  });

  const continueButton = $(".continue-btn");
  const steps = $(".section-survey");
  const signUpButton = $(".sign-up-btn");
  const backItem = $("#backItem");
  let currentStep = 1;
  let isBack = false;

  continueButton.on("click", function () {
    if (currentStep < steps.length) {
      if (validateStep(currentStep)) {
        $(steps[currentStep - 1]).removeClass("active").addClass("hidden");
        $(steps[currentStep]).addClass("active").removeClass("hidden");

        for (let i = currentStep + 1; i < steps.length; i++) {
          $(steps[i]).removeClass("active").addClass("hidden");
        }
        validateStep(currentStep + 1);
        currentStep++;
        if (currentStep !== 1) {
          $(backItem).css("display", "flex");
        } else {
          $(backItem).css("display", "none");
        }
        if (currentStep === 4) {
          $(signUpButton).removeClass("hidden");
          $(continueButton).addClass("hidden");
        } else {
          $(signUpButton).addClass("hidden");
          $(continueButton).removeClass("hidden");
        }
      }
    }
  });

  function validateStep(step) {
    switch (step) {
      case 1:
        if (!$("#Email").val()) {
          $(continueButton).prop("disabled", true);
        } else {
          handleEmailInput();
        }
        return true;
        break;
      case 2:
        const FirstNameValue = $("#FirstName").val();
        const LastNameValue = $("#LastName").val();
        const BirthdayValue = $("#Birthday").val();
        if (!FirstNameValue || !LastNameValue || !BirthdayValue) {
          $(continueButton).prop("disabled", true);
        } else {
          validateBirthday();
          validateLastName();
          validateFirstName();
        }
        return true;
        break;
      case 3:
        const newPasswordValue = $("#Password").val();
        const confirmationPasswordValue = $("#ConfirmPassword").val();
        if (!newPasswordValue || !confirmationPasswordValue) {
          $(continueButton).prop("disabled", true);
        } else {
          validateConfirmationPassword();
          validateNewPassword();
        }
        return true;
        break;
      case 4:
        return true;
        break;
      default:
        break;
    }
  }

  $("#backItem").on("click", function () {
    validateStep(currentStep - 1);
    isBack === true;
    currentStep--;
    if (currentStep > 1) {
      $(backItem).css("display", "flex");
    } else {
      $(backItem).css("display", "none");
    }
    $(signUpButton).addClass("hidden");
    for (let i = currentStep + 1; i < steps.length; i++) {
      $(steps[i]).removeClass("active").addClass("hidden");
    }
    $(steps[currentStep]).removeClass("active").addClass("hidden");
    $(steps[currentStep - 1]).addClass("active").removeClass("hidden");
    if (currentStep === 4) {
      $(signUpButton).removeClass("hidden");
      $(continueButton).addClass("hidden");
    } else {
      $(signUpButton).addClass("hidden");
      $(continueButton).removeClass("hidden");
    }
  });

  const EmailInput = $("#Email");
  const FirstNameInput = $("#FirstName");
  const LastNameInput = $("#LastName");
  const BirthdayInput = $("#Birthday");
  const newPasswordInput = $("#Password");
  const confirmationPasswordInput = $("#ConfirmPassword");
  const passwordMeter = $("#passwordMeter");
  const meterText = $("#meter-text");
  const ruleList = $("#rule-list");
  const ruleItems = $(".rule-item");
  const passLengthItem = $("#pass-length");
  const passNumberItem = $("#pass-number");
  const passUpCaseItem = $("#pass-Up-case");
  const passSymboItem = $("#pass-symbo");
  const referralCodeElement = $("#ReferralCode");
  const policyCheckbox = $("#policy");

  EmailInput.on("input", handleEmailInput);

  function handleEmailInput() {
    const Email = EmailInput.val();
    const isValidEmail = validateEmail(Email);

    if (!isValidEmail) {
      EmailInput.css("border-color", "#d98c93");
      $(continueButton).prop("disabled", true);
    } else {
      EmailInput.css("border-color", "");
      $(continueButton).prop("disabled", false);
    }
  }

  function validateEmail(Email) {
    const EmailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return EmailPattern.test(Email);
  }

  FirstNameInput.on("input", validateFirstName);
  LastNameInput.on("input", validateLastName);
  BirthdayInput.on("input", validateBirthday);

  function validateFirstName() {
    const FirstName = FirstNameInput.val().trim();
    const isValidFirstName = FirstName !== "";

    if (!isValidFirstName) {
      FirstNameInput.css("border-color", "#d98c93");
    } else {
      FirstNameInput.css("border-color", "");
    }

    updateContinueButtonState();
  }

  function validateLastName() {
    const LastName = LastNameInput.val().trim();
    const isValidLastName = LastName !== "";

    if (!isValidLastName) {
      LastNameInput.css("border-color", "#d98c93");
    } else {
      LastNameInput.css("border-color", "");
    }

    updateContinueButtonState();
  }

  function validateBirthday() {
    const Birthday = BirthdayInput.val().trim();
    const isValidBirthday = Birthday !== "";

    if (!isValidBirthday) {
      BirthdayInput.css("border-color", "#d98c93");
    } else {
      BirthdayInput.css("border-color", "");
    }

    updateContinueButtonState();
  }

  function updateContinueButtonState() {
    const isValidFirstName = FirstNameInput.val().trim() !== "";
    const isValidLastName = LastNameInput.val().trim() !== "";
    const isValidBirthday = BirthdayInput.val().trim() !== "";

    $(continueButton).prop(
      "disabled",
      !(isValidFirstName && isValidLastName && isValidBirthday)
    );
  }

  newPasswordInput.on("input", validateNewPassword);
  confirmationPasswordInput.on("input", validateConfirmationPassword);

  function validateNewPassword() {
    const newPassword = newPasswordInput.val();
    const confirmationPassword = confirmationPasswordInput.val();

    const isValidNewPassword = validatePasswordFormat(newPassword);
    passwordMeter.css("width", `${isValidNewPassword}%`);
    if (isValidNewPassword > 80 && isValidNewPassword <= 100) {
      meterText.text("Strong");
      passwordMeter.css("background-color", "#3E8E41");
    } else if (isValidNewPassword > 40 && isValidNewPassword <= 80) {
      meterText.text("Good");
      passwordMeter.css("background-color", "#EC9213");
    } else {
      meterText.text("Weak");
      passwordMeter.css("background-color", "#C0404C");
    }
    if (newPasswordInput.val()) {
      ruleList.css("display", "grid");
    }
    const isValidConfirmationPassword =
      newPassword === confirmationPassword || confirmationPassword === "";
    if (isValidNewPassword !== 100) {
      newPasswordInput.css("border-color", "#d98c93");
    } else {
      newPasswordInput.css("border-color", "");
    }
    if (!isValidConfirmationPassword) {
      confirmationPasswordInput.css("border-color", "#d98c93");
    } else {
      confirmationPasswordInput.css("border-color", "");
    }
    if (newPassword.length >= 8) {
      passLengthItem.css("color", "var(--white-color)");
      passLengthItem
        .find("img")
        .attr("src", "/assets/icons/check-icon.svg");
    } else {
      passLengthItem.css("color", "var(--disable-text-color)");
      passLengthItem
        .find("img")
        .attr("src", "/assets/icons/close-icon.svg");
    }

    if (/\d/.test(newPassword)) {
      passNumberItem.css("color", "var(--white-color)");
      passNumberItem
        .find("img")
        .attr("src", "/assets/icons/check-icon.svg");
    } else {
      passNumberItem.css("color", "var(--disable-text-color)");
      passNumberItem
        .find("img")
        .attr("src", "/assets/icons/close-icon.svg");
    }

    if (/[A-Z]/.test(newPassword) && /[a-z]/.test(newPassword)) {
      passUpCaseItem.css("color", "var(--white-color)");
      passUpCaseItem
        .find("img")
        .attr("src", "/assets/icons/check-icon.svg");
    } else {
      passUpCaseItem.css("color", "var(--disable-text-color)");
      passUpCaseItem
        .find("img")
        .attr("src", "/assets/icons/close-icon.svg");
    }

    if (/[!@#$%^&*(),.?":{}|<>]/.test(newPassword)) {
      passSymboItem.css("color", "var(--white-color)");
      passSymboItem
        .find("img")
        .attr("src", "/assets/icons/check-icon.svg");
    } else {
      passSymboItem.css("color", "var(--disable-text-color)");
      passSymboItem
        .find("img")
        .attr("src", "/assets/icons/close-icon.svg");
    }
    updateContinueButtonStatePass();
  }

  function validateConfirmationPassword() {
    const newPassword = newPasswordInput.val();
    const confirmationPassword = confirmationPasswordInput.val();

    const isValidConfirmationPassword =
      newPassword === confirmationPassword || confirmationPassword === "";

    if (!isValidConfirmationPassword && confirmationPassword !== "") {
      confirmationPasswordInput.css("border-color", "#d98c93");
    } else {
      confirmationPasswordInput.css("border-color", "");
    }

    updateContinueButtonStatePass();
  }

  function validatePasswordFormat(password) {
    const conditions = [
      /[A-Z]/.test(password),
      /[a-z]/.test(password),
      /\d/.test(password),
      /[!@#$%^&*(),.?":{}|<>]/.test(password),
      password.length >= 8,
    ];

    const totalConditions = conditions.length;
    let correctConditions = conditions.filter(condition => condition).length;

    const percentage = (correctConditions / totalConditions) * 100;
    return Math.round(percentage);
  }

  function updateContinueButtonStatePass() {
    const isValidNewPassword = validatePasswordFormat(newPasswordInput.val());
    const isValidConfirmationPassword =
      newPasswordInput.val() === confirmationPasswordInput.val();

    $(continueButton).prop(
      "disabled",
      !(isValidNewPassword && isValidConfirmationPassword)
    );
  }

  //policyCheckbox.on("change", function () {
  //  if (policyCheckbox.prop("checked")) {
  //    signUpButton.prop("disabled", false);
  //  } else {
  //    signUpButton.prop("disabled", true);
  //  }
  //});

}(jQuery));	

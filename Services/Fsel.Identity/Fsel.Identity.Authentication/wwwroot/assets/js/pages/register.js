(function ($) {
  const dateOfBirthInput = document.getElementById("birthday");
  const $phoneNumberInput = $("#phoneNumber");
  const $firstNameInput = $("#firstName");
  const $lastNameInput = $("#lastName");
  const $gender = $("#gender");
  const $passwordMock = $("#password-mock");
  const $passwordMeter = $("#meter");
  const $meterText = $("#meter-text");
  const $ruleList = $("#rule-list");
  const $passLengthItem = $("#pass-length");
  const $passNumberItem = $("#pass-number");
  const $passUpCaseItem = $("#pass-Up-case");
  const $passSymboItem = $("#pass-symbo");
  const $policyCheckbox = $("#policy");
  const $genderRadios = $("input[name='gender']");
  const $policy = $(".service-policy");

  $phoneNumberInput.on("input", () => isValid($phoneNumberInput));
  $firstNameInput.on("input", () => isValid($firstNameInput));
  $lastNameInput.on("input", () => isValid($lastNameInput));
  $passwordMock.on("input", validateNewPassword);
  dateOfBirthInput.addEventListener("input", runValidateDateOfBirth);
  dateOfBirthInput.addEventListener("change", runValidateDateOfBirth);
  $policyCheckbox.on("change", () => isValidPolicy());

  $("#form-Register").on("submit", function (e) {
    if (validateRegisterForm() && this.checkValidity()) {
      $("#userInfoContain").addClass("hidden-form");
      $("#createAccountContain").addClass("show-form");
      e.preventDefault();
    }
    e.preventDefault();
  });

  $("#form-Password").on("submit", function (e) {
    if (validateAll() && this.checkValidity()) {
      e.preventDefault();
      var sourceData = $("#form-Register").serializeArray();
      $.each(sourceData, function (i, field) {
        if (field.value) {
          $("#form-Password input[name=" + field.name + "]").val(field.value);
        }
      });;
      this.submit();
    }
    e.preventDefault();
  });

  dateOfBirthInput.addEventListener("input", function (e) {
    let value = e.target.value.replace(/\D/g, "");

    if (value.length > 8) {
      value = value.slice(0, 8);
    }

    if (value.length > 1 && value.length <= 3) {
      value = value.replace(/^(\d{2})(\d{0,2})$/, "$1/$2");
    }
    else if (value.length > 3) {
      value = value.replace(/^(\d{2})(\d{2})(\d{0,4})$/, "$1/$2/$3");
    }

    e.target.value = value;
  });
  function isValid($queryElement) {
    if (!$queryElement) {
      return false;
    }
    const isValid = $queryElement.valid();
    if (isValid) {
      $queryElement.removeClass("content-border-danger");
    } else {
      $queryElement.addClass("content-border-danger");
    }
    return isValid;
  }
  function isValidPolicy() {
    const isPolicyChecked = $policyCheckbox.is(":checked");
    if (!isPolicyChecked) {
      const $label = $('label[for="policy"]');
      $label.addClass("content-border-danger");
      $policy.addClass("content-text-danger");
    }
    return isPolicyChecked;
  }

  function validatePasswordFormat() {
    const isValid = $passwordMock.valid();

    if (!isValid) {
      $passwordMock.addClass("content-border-danger");
    } else {
      $passwordMock.removeClass("content-border-danger");
    }
    return isValid;
  }

  function validateNewPassword() {
    const passwordValue = $passwordMock.val();
    const isValidNewPassword = validatePasswordFormat();
    $passwordMeter.css("width", `${isValidNewPassword}%`);
    if (passwordValue > 80) {
      $meterText.text($meterText.attr("text-strong"));
      $passwordMeter.css("background-color", "#3E8E41");
    } else if (passwordValue > 40) {
      $meterText.text($meterText.attr("text-good"));
      $passwordMeter.css("background-color", "#EC9213");
    } else {
      $meterText.text($meterText.attr("text-weak"));
      $passwordMeter.css("background-color", "#C0404C");
    }

    if (passwordValue) {
      $ruleList.css("display", "grid");
    } else {
      $meterText.text("");
    }
    updatePasswordRules(passwordValue);
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
  function runValidateDateOfBirth() {
    const isValid = validateDateOfBirth();
    dateOfBirthInput.classList.remove("content-border-danger");
    if (!isValid) {
      dateOfBirthInput.classList.add("content-border-danger");
      return false;
    }
    return true;
  }

  function validateDateOfBirth() {
    const value = dateOfBirthInput.value.trim();
    if (!value) {
      return false;
    }

    function parseDateFromDDMMYYYY(value) {
      const regex = /^(\d{2})\/(\d{2})\/(\d{4})$/;
      const match = value.match(regex);
      if (!match) return null;

      const day = parseInt(match[1], 10);
      const month = parseInt(match[2], 10) - 1;
      const year = parseInt(match[3], 10);

      const date = new Date(year, month, day);
      if (
        date.getFullYear() !== year ||
        date.getMonth() !== month ||
        date.getDate() !== day
      ) {
        return null;
      }
      return date;
    }

    const date = parseDateFromDDMMYYYY(value);
    const today = new Date();

    if (!date) {
      return false;
    }

    if (date > today) {
      return false;
    }

    return true;
  }
  function validateRegisterForm() {
    const isValidFirstName = isValid($firstNameInput);
    const isValidLastName = isValid($lastNameInput);
    const isValidPhoneNumber = isValid($phoneNumberInput);
    const isValidBirthday = validateDateOfBirth();
    const isCheckedPolicy = isValidPolicy();
    return isValidFirstName && isValidLastName && isValidPhoneNumber && isValidBirthday && isCheckedPolicy;
  }
  function validateAll() {
    const isValidRegister = validateRegisterForm();
    if (!isValidRegister) {
      return isValidRegister;
    }

    const isValidPassword = $passwordMock.valid();
    if (!isValidPassword) {
      return isValidPassword;
    }
    return true;
  }

  $(window).on("pageshow", function () {
    var gender = $gender.attr("value");
    if (!gender) {
      $genderRadios.prop("checked", false);
    }
    $policyCheckbox.prop("checked", false);

    const isShowVerifyOtp = $("input[name='IsShowVerifyOtp']").val();
    if (isShowVerifyOtp) {
      $phoneNumberInput.val("");
      $firstNameInput.val("");
      $lastNameInput.val("");
      $passwordMock.val("");
      $policyCheckbox.prop("checked", false);
      $genderRadios.prop("checked", false);
    }
  });
})(jQuery);

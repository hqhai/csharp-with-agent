(function ($) {

  $(document).ready(function () {
    $("#birthday").flatpickr({
      dateFormat: "d/m/y",
      maxDate: "today"
    });
  });

  $.validator.addMethod(
    "daterequired",
    function (value, element) {
      return value != null && value != undefined && value;
    },
    "Date is required"
  );

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
      });
      $("#form-Password input[name=Gender]").val(getGenderValue());
      this.submit();
    }
    e.preventDefault();
  });



  const $phoneNumberInput = $("#phoneNumber");
  const $firstNameInput = $("#firstName");
  const $lastNameInput = $("#lastName");
  const $gender = $("#gender");
  const $birthday = $("#birthday");
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
  const $optionGender = $(".option");
  const $radioLabels = $(".round-radio-label");

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

  function validateGender() {
    let selectedGender;
    $genderRadios.each(function () {
      if ($(this).is(":checked")) {
        selectedGender = $(this).val();
        return false; // break the loop
      }
    });

    if (selectedGender) {
      $optionGender.removeClass("content-text-danger content-border-danger");
      $radioLabels.each(function () {
        $(this).removeClass("content-border-danger");
        $(this).addClass("content-border-success"); // Assuming you have a success class for the desired style
      });
    } else {
      $optionGender.addClass("content-text-danger content-border-danger");
      $radioLabels.each(function () {
        $(this).removeClass("content-border-success");
        $(this).addClass("content-border-danger");
      });
    }

    return selectedGender;
  }

  function getGenderValue() {
    let selectedGender;
    $genderRadios.each(function () {
      if ($(this).is(":checked")) {
        selectedGender = $(this).val();
        return false; // break the loop
      }
    });
    return selectedGender;
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

    // Update password rules
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

  function validateRegisterForm() {
    const isValidFirstName = isValid($firstNameInput);
    const isValidLastName = isValid($lastNameInput);
    const isValidPhoneNumber = isValid($phoneNumberInput);
    const isValidBirthday = isValid($birthday);
    const isValidGender = validateGender();
    const isCheckedPolicy = isValidPolicy();
    return isValidFirstName && isValidLastName && isValidPhoneNumber && isValidBirthday && isValidGender && isCheckedPolicy;
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

  $phoneNumberInput.on("input", () => isValid($phoneNumberInput));
  $firstNameInput.on("input", () => isValid($firstNameInput));
  $lastNameInput.on("input", () => isValid($lastNameInput));
  $passwordMock.on("input", validateNewPassword);
  $genderRadios.on("change", isGenderSelected);
  $policyCheckbox.on("change", () => isValidPolicy());

  function isGenderSelected() {
    $optionGender.removeClass("content-text-danger content-border-danger");
    $radioLabels.each(function () {
      $(this).removeClass("content-border-danger").addClass("content-border-success");
    });

    // Kiểm tra xem radio button giới tính đã được chọn chưa
    return $genderRadios.is(":checked");
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

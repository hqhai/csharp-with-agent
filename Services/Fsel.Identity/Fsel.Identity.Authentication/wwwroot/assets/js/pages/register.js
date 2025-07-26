(function ($) {
  $('#form-Register').on('submit', function (e) {
    if (validateAll() && this.checkValidity()) {
      $('.loading').removeClass('hidden');
    }
    else {
      e.preventDefault();
    }
  });

  // Input elements
  const $steps = $(".section-survey");
  const $signUpButton = $(".sign-up-btn");
/*  const $emailInput = $("#email");*/
  const $phoneNumberInput = $("#phoneNumber");
  const $firstNameInput = $("#firstName");
  const $lastNameInput = $("#lastName");
  const $newPasswordInput = $("#password");
  const $passwordMeter = $("#passwordMeter");
  const $meterText = $("#meter-text");
  const $ruleList = $("#rule-list");
  const $ruleItems = $ruleList.find(".rule-item");
  const $passLengthItem = $("#pass-length");
  const $passNumberItem = $("#pass-number");
  const $passUpCaseItem = $("#pass-Up-case");
  const $passSymboItem = $("#pass-symbo");
  const $referralCodeElement = $("#referral-code");
  const $policyCheckbox = $("#policy");
  const $dayInput = $("#Day");
  const $monthBirthdayInput = $("#MonthBirthday");
  const $monthInput = $("#Month");
  const $yearInput = $("#Year");
  const $genderRadios = $("input[name='gender']");
  const $gender = $("#gender");
  const $policy = $(".service-policy");
  const $optionGender = $(".option");
  const $radioButtons = $(".round-radio");
  const $radioLabels = $(".round-radio-label");

  // Validation functions
  function validateEmail() {
    const email = $emailInput.val();
    const emailPattern = /^(?=.{1,64}@)(?=.{1,255}$)[a-zA-Z0-9!#$%&'*+/=?^_{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?$/;
    if (email !== "" && emailPattern.test(email)) {
      $emailInput.removeClass("content-border-danger");
      return true;
    } else {
      $emailInput.addClass("content-border-danger");
      return false;
    }
  }

  function validatePhoneNumber() {
    const phone = $phoneNumberInput.val().trim();
    const phonePattern = /^\+?\d{7,15}$/;
    if (phone !== "" && phonePattern.test(phone)) {
      $phoneNumberInput.removeClass("content-border-danger");
      return true;
    } else {
      $phoneNumberInput.addClass("content-border-danger");
      return false;
    }
  }

  function validateGender() {
    // gender
    let selectedGender;
    $genderRadios.each(function () {
      if ($(this).is(':checked')) {
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
    }
    else {
      $optionGender.addClass("content-text-danger content-border-danger");
      $radioLabels.each(function () {
        $(this).removeClass("content-border-success");
        $(this).addClass("content-border-danger");
      });
    }

    return selectedGender;
  }
  function validateFirstName() {
    const firstName = $firstNameInput.val().trim();
    if (firstName === "") {
      $firstNameInput.addClass("content-border-danger");
      return false;
    } else {
      $firstNameInput.removeClass("content-border-danger");
      return true;
    }
  }

  function validateLastName() {
    const lastName = $lastNameInput.val().trim();
    if (lastName === "") {
      $lastNameInput.addClass("content-border-danger");
      return false;
    } else {
      $lastNameInput.removeClass("content-border-danger");
      return true;
    }
  }

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
      $newPasswordInput.addClass("content-border-danger")
    } else {
      $newPasswordInput.removeClass("content-border-danger");
    }

    // Update password rules
    updatePasswordRules(newPassword);
    updateContinueButtonStatePass();
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

  function validateDay() {
    var isValidDay = $dayInput.val().trim() !== "";
    if (!isValidDay) {
      $dayInput.addClass("content-border-danger");
    }
    else {
      $dayInput.removeClass("content-border-danger");
    }
    return isValidDay && validateDate();
  }
  
  function validateMonth() {
    var isValidMonth = $monthBirthdayInput.val().trim() !== "";
    if (!isValidMonth) {
      $monthInput.addClass("content-border-danger");
    }
    else {
      $monthInput.removeClass("content-border-danger");
    }
    return isValidMonth && validateDate();
  }
  
  function validateYear() {
    var isValidYear = $yearInput.val().trim() !== "";
    if (!isValidYear) {
      $yearInput.addClass("content-border-danger");
    }
    else {
      $yearInput.removeClass("content-border-danger");
    }
    return isValidYear && validateDate();
  }

  function validateDate() {
    if ($yearInput.val().trim() !== "" && $monthBirthdayInput.val().trim() !== "" && $dayInput.val().trim() !== "") {
      var year = parseInt($yearInput.val().trim());
      var month = parseInt($monthBirthdayInput.val().trim()) - 1;
      var day = parseInt($dayInput.val().trim());

      const date = new Date(year, month, day);
      const currentDate = new Date();
      if (date.getFullYear() === year && date.getMonth() === month && date.getDate() === day && date <= currentDate) {
        $yearInput.removeClass("content-border-danger");
        $monthInput.removeClass("content-border-danger");
        $dayInput.removeClass("content-border-danger");
        $("input.text-input-hidden[name='Birthday']").attr("value", `${year}-${month}-${day}`)
        $("input.text-input-hidden[name='Birthday']").valid();
        $("input.text-input-hidden[name='BirthdayStr']").attr("value", "BirthdayStr");
        $("input.text-input-hidden[name='BirthdayStr']").valid();
      }
      else {
        $yearInput.addClass("content-border-danger");
        $monthInput.addClass("content-border-danger");
        $dayInput.addClass("content-border-danger");
        $("input.text-input-hidden[name='Birthday']").attr("value", `${year}-${month}-${day}`)
        $("input.text-input-hidden[name='Birthday']").valid();
        $("input.text-input-hidden[name='BirthdayStr']").attr("value", "Birthday");
        $("input.text-input-hidden[name='BirthdayStr']").valid();
      }
    }
    return true;
  }

  function validateAll() {
    let isDoneAllValidate = true;

    // Validate each field
    const isValidFirstName = validateFirstName();
    const isValidLastName = validateLastName();
/*    const isValidEmail = validateEmail();*/
    const isValidPhoneNumber = validatePhoneNumber();
    const isValidGender = validateGender();
    const isValidDay = validateDay();
    const isValidMonth = validateMonth();
    const isValidYear = validateYear();
    const isValidPassword = validatePasswordFormat() === 100;
    const isPolicyChecked = $policyCheckbox.is(":checked");

    if (!isValidFirstName) isDoneAllValidate = false;
    if (!isValidLastName) isDoneAllValidate = false;
   /* if (!isValidEmail) isDoneAllValidate = false;*/
    if (!isValidPhoneNumber) isDoneAllValidate = false;
    if (!isValidGender) isDoneAllValidate = false;
    if (!isValidDay || !isValidMonth || !isValidYear) isDoneAllValidate = false;
    if (!isValidPassword) isDoneAllValidate = false;
    if (!isPolicyChecked) isDoneAllValidate = false;


    // Update UI for policy checkbox
    if (!isPolicyChecked) {
      const $label = $('label[for="policy"]');
      $label.addClass("content-border-danger");
      $policy.addClass("content-text-danger");
    }

    return isDoneAllValidate;
  }

  function updateContinueButtonStatePass() {
    const isValidNewPassword = validatePasswordFormat($newPasswordInput.val()) === 100;
    // $continueButton.prop('disabled', !isValidNewPassword);
  }

  function checkFormValidity() {
    validateAll();
    // Check form validity and enable/disable continue button
    // $continueButton.prop('disabled', !validateAll());
  }

  // Event listeners
/*  $emailInput.on("input", validateEmail);*/
  $phoneNumberInput.on("input", validatePhoneNumber);
  $firstNameInput.on("input", validateFirstName);
  $lastNameInput.on("input", validateLastName);
  $newPasswordInput.on("input", validateNewPassword);
  $dayInput.on("input", validateDay);
  $monthInput.on("input", validateMonth);
  $yearInput.on("input", validateYear);

  //$genderRadios.on("change", checkFormValidity);
  $genderRadios.on("change", isGenderSelected);

  $policyCheckbox.on("change", function () {
    const $label = $('label[for="policy"]');
    $label.removeClass("content-border-danger")
    $policy.removeClass("content-text-danger")
  });

  function selectOptionMonth(inputId, inputValueId, option, value) {
    $("#" + inputValueId).attr("value", value);
    selectOption(inputId, option);
  }

  function selectOption(inputId, option) {
    // Gán giá trị lựa chọn vào phần tử input
    $("#" + inputId).attr("value", option);

    // Ẩn các dropdown menu
    $("#dayDropdownContent").hide();
    $("#monthDropdownContent").hide();
    $("#yearDropdownContent").hide();

    // Đánh dấu lựa chọn đã chọn
    const $dropdownContent = $("#" + inputId.toLowerCase() + "DropdownContent");
    $dropdownContent.find("div").removeClass("selected");
    $(event.target).addClass("selected");
  }

  $dayInput.on("click", function () {
    $("#dayDropdownContent").show();
    //$dayInput.css("border-color", "");
  });

  $monthInput.on("click", function () {
    $("#monthDropdownContent").show();
    //$monthInput.css("border-color", "");
  });

  $yearInput.on("click", function () {
    $("#yearDropdownContent").show();
    //$yearInput.css("border-color", "");
  });

  // Sự kiện click bên ngoài dropdown menu để ẩn nó
  $(document).on("click", function (event) {
    if (!$(event.target).is($dayInput) && !$(event.target).closest(".dropdown-content").length) {
      $("#dayDropdownContent").hide();
      //$("#dayDropdownContent div").css({
      //  "background-color": "initial",
      //  "color": "initial"
      //});
    }

    if (!$(event.target).is($monthInput) && !$(event.target).closest(".dropdown-content").length) {
      $("#monthDropdownContent").hide();
      //$("#monthDropdownContent div").css({
      //  "background-color": "initial",
      //  "color": "initial"
      //});
    }

    if (!$(event.target).is($yearInput) && !$(event.target).closest(".dropdown-content").length) {
      $("#yearDropdownContent").hide();
      //$("#yearDropdownContent div").css({
      //  "background-color": "initial",
      //  "color": "initial"
      //});
    }
  });

  // Lấy ngày tháng năm hiện tại
  const currentDate = new Date();
  const currentDay = currentDate.getDate();
  const currentMonth = currentDate.getMonth() + 1; // Tháng tính từ 0
  const currentYear = currentDate.getFullYear();

  // Tạo mảng chứa các tùy chọn ngày, tháng, năm
  const dayOptions = [];
  const monthOptions = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
  const yearOptions = [];

  // Tạo các tùy chọn ngày (1-31)
  for (let i = 1; i <= 31; i++) {
    dayOptions.push(i.toString());
  }

  // Tạo các tùy chọn năm (chỉ những năm mà tuổi của người dùng sẽ là 100 tuổi)
  for (let i = 0; i < 100; i++) {
    yearOptions.push((currentYear - i).toString());
  }

  // Tạo các phần tử dropdown
  const $dayDropdownContent = $("#dayDropdownContent");
  const $monthDropdownContent = $("#monthDropdownContent");
  const $yearDropdownContent = $("#yearDropdownContent");

  // Thêm các tùy chọn vào các dropdown
  dayOptions.forEach(function (option) {
    $("<div>").addClass("dropdown-option")
      .text(option)
      .appendTo($dayDropdownContent)
      .on("click", function () {
        selectOption('Day', option);
        validateDay();
      });
  });

  $('.MonthBirthday-values').each(function () {
    var months = $(this).attr('data-values').split(',');
    months.forEach(function (option, index) {
      var curMonth = $("#Month").attr("value");
      if (curMonth && curMonth == index + 1){
        $("#Month").attr("value", option);
      }

      $("<div>").addClass("dropdown-option")
        .text(option)
        .appendTo($monthDropdownContent)
        .on("click", function () {
          selectOptionMonth('Month', 'MonthBirthday', option, index + 1);
          validateMonth();
        });
    });
  });

  yearOptions.forEach(function (option) {
    $("<div>").addClass("dropdown-option")
      .text(option)
      .appendTo($yearDropdownContent)
      .on("click", function () {
        selectOption('Year', option);
        validateYear();
      });
  });

  function isGenderSelected() {
    $optionGender.removeClass("content-text-danger content-border-danger");
    $radioLabels.each(function () {
      $(this).removeClass("content-border-danger").addClass("content-border-success");
    });

    // Kiểm tra xem radio button giới tính đã được chọn chưa
    return $genderRadios.is(":checked");
  }

  $(window).on('pageshow', function () {
    var gender = $gender.attr("value");
    if (!gender) {
      $genderRadios.prop('checked', false);
    }
    $policyCheckbox.prop('checked', false);

    const isShowVerifyOtp = $("input[name='IsShowVerifyOtp']").val();
    if (isShowVerifyOtp) {
      $emailInput.val('');
      $phoneNumberInput.val('');
      $firstNameInput.val('');
      $lastNameInput.val('');
      $newPasswordInput.val('');
      $dayInput.val('');
      $monthInput.val('');
      $monthBirthdayInput.val('');
      $yearInput.val('');

      $policyCheckbox.prop('checked', false);
      $genderRadios.prop('checked', false);
    }
  });

}(jQuery));	

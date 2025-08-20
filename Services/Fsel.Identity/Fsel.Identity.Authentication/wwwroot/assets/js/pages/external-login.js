(function ($) {
  const dateOfBirthInput = document.getElementById("birthday");
  const $phoneNumberInput = $("#phoneNumber");
  const $firstNameInput = $("#firstName");
  const $lastNameInput = $("#lastName");
  const $policyCheckbox = $("#policy");
  const $genderRadios = $("input[name='gender']");
  const $gender = $("#gender");
  const $policy = $(".service-policy");

  $phoneNumberInput.on("input", validatePhoneNumber);
  $firstNameInput.on("input", validateFirstName);
  $lastNameInput.on("input", validateLastName);
  dateOfBirthInput.addEventListener("input", runValidateDateOfBirth);
  dateOfBirthInput.addEventListener("change", runValidateDateOfBirth);

  $policyCheckbox.on("change", function () {
    const $label = $('label[for="policy"]');
    $label.removeClass("content-border-danger")
    $policy.removeClass("content-text-danger")
  });
  dateOfBirthInput.addEventListener("input", function (e) {
    let value = e.target.value.replace(/\D/g, "");

    if (value.length > 8) value = value.slice(0, 8);

    if (value.length >= 4) {
      value = value.replace(/^(\d{2})(\d{2})(\d{0,4})$/, "$1/$2/$3");
    } else if (value.length >= 2) {
      value = value.replace(/^(\d{2})(\d{0,2})$/, "$1/$2");
    }

    e.target.value = value;
  });

  dateOfBirthInput.addEventListener("keydown", function (e) {
    if (e.key === "Backspace") {
      const pos = e.target.selectionStart;
      const val = e.target.value;
      if (pos > 0 && val[pos - 1] === "/") {
        e.preventDefault();
        e.target.value = val.slice(0, pos - 1) + val.slice(pos);
        e.target.setSelectionRange(pos - 1, pos - 1);
      }
    }
  });

  $('#form-ExternalLogin').on('submit', function (e) {
    if (validateAll() && this.checkValidity()) {
      $('.loading').removeClass('hidden');
      let $birthdayInput = $(this).find('[name="Birthday"]');
      let originalValue = $birthdayInput.val(); 

      let parts = originalValue.split("/");
      if (parts.length === 3) {
        $birthdayInput.val(`${parts[2]}-${parts[1].padStart(2, '0')}-${parts[0].padStart(2, '0')}`);
      }
      setTimeout(() => {
        $birthdayInput.val(originalValue);
      }, 0);
    }
    else {
      e.preventDefault();
    }
  });
  $.validator.methods.required = function (value, element, param) {
    if (typeof value === "string") {
      value = $.trim(value);
    }
    return value.length > 0;
  }

  function validatePhoneNumber() {
    $('input[name="PhoneNumber"]').valid();
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

  function validateFirstName() {
    const firstName = $firstNameInput.val().trim();
    $('input[name="FirstName"]').valid();
    if (firstName === "") {
      $firstNameInput.addClass("content-border-danger");
      return false;
    } else {
      $firstNameInput.removeClass("content-border-danger");
      return true;
    }
  }

  function validateLastName() {
    $('input[name="LastName"]').valid();
    const lastName = $lastNameInput.val().trim();
    if (lastName === "") {
      $lastNameInput.addClass("content-border-danger");
      return false;
    } else {
      $lastNameInput.removeClass("content-border-danger");
      return true;
    }
  }

  function validateAll() {
    let isDoneAllValidate = true;

    // Validate each field
    const isValidFirstName = validateFirstName();
    const isValidLastName = validateLastName();
    const isValidPhoneNumber = validatePhoneNumber();
    const isValidBirthday = validateDateOfBirth();
    const isPolicyChecked = $policyCheckbox.is(":checked");

    if (!isValidFirstName) isDoneAllValidate = false;
    if (!isValidLastName) isDoneAllValidate = false;
    if (!isValidPhoneNumber) isDoneAllValidate = false;
    if (!isValidBirthday) isDoneAllValidate = false;
    if (!isPolicyChecked) isDoneAllValidate = false;

    if (!isPolicyChecked) {
      const $label = $('label[for="policy"]');
      $label.addClass("content-border-danger");
      $policy.addClass("content-text-danger");
    }

    return isDoneAllValidate;
  }

  function runValidateDateOfBirth() {
    const value = dateOfBirthInput.value.trim();
    if (isNullOrWhiteSpace(value)) {
      let emptyMessage = $('div.validation-message-text[data-field="Birthday"][data-validate="data-val-required"]').text();
      addInvalidMessage(emptyMessage);
    }
    else {
      const isValid = validateDateOfBirth()
      dateOfBirthInput.classList.remove("content-border-danger");
      if (!isValid) {
        dateOfBirthInput.classList.add("content-border-danger");
        let invalidMessage = $("#invalidBirthday").attr("data-value")
        addInvalidMessage(invalidMessage);
        return false;
      }
      $('span[data-valmsg-for="Birthday"] #birthday-error').remove();
      return true;
    }
  }
  function addInvalidMessage(message) {
    const $parentSpan = $('span[data-valmsg-for="Birthday"]');
    let $errorSpan = $parentSpan.find("#birthday-error");

    if ($errorSpan.length > 0) {
      $errorSpan.text(message);
    } else {
      $parentSpan.append('<span id="birthday-error">' + message + '</span>');
    }
  }
  function isNullOrWhiteSpace(input) {
    return !((input ?? "").trim());
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


  $(window).on('pageshow', function () {
    var gender = $gender.attr("value");
    if (!gender) {
      $genderRadios.prop('checked', false);
    }
    $policyCheckbox.prop('checked', false);
  });
}(jQuery));

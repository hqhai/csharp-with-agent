(function ($) {
  $('#form-Forgot').on('submit', function (e) {
    if (validateAll() && this.checkValidity()) {
      $('.loading').removeClass('hidden');
    }
    else {
      e.preventDefault();
    }
  });

  const emailInput = $("#email");

  emailInput.on("input", validateEmail);

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

  function validateAll() {
    let isDoneAllValidate = true;

    const isValidEmail = validateEmail();
    if (!isValidEmail) isDoneAllValidate = false;

    return isDoneAllValidate;
  }

}(jQuery));	

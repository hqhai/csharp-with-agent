(function ($) {
  $('#form-Forgot').on('submit', function (e) {
    if (validateAll() && this.checkValidity()) {
      $('.loading').removeClass('hidden');
    }
    else {
      e.preventDefault();
    }
  });

  const identityInput = $("#identity");

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

  function validateAll() {
    let isDoneAllValidate = true;

    const isValidEmail = validateEmail();
    if (!isValidEmail) isDoneAllValidate = false;

    return isDoneAllValidate;
  }

}(jQuery));	

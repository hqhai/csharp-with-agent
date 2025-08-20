(function ($) {
  $('#form-Forgot').on('submit', function (e) {
    if (validateAll() && this.checkValidity()) {
      $('.loading').removeClass('hidden');
    }
    else {
      e.preventDefault();
    }
  });

  $('#form-Forgot').validate({
    onfocusout: true 
  });

  $.validator.methods.required = function (value, element, param) {
    if (typeof value === "string") {
      value = $.trim(value);
    }
    return value.length > 0;
  }
  const identityInput = $("#identity");

  identityInput.on("input", validateEmail);

  function validateEmail() {
    const identity = identityInput.val();
    const emailPattern = /^(?=.{1,64}@)(?=.{1,255}$)[a-zA-Z0-9!#$%&'*+/=?^_{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?$/;
    const phonePattern = /^\+?\d{7,15}$/;
    $('span[data-valmsg-for="Identity"]').empty();
    if (!isNullOrWhiteSpace(identity)) {
      if (phonePattern.test(identity)) {
        identityInput.removeClass("content-border-danger");
        return true;
      }
      if (emailPattern.test(identity)) {
        identityInput.removeClass("content-border-danger");
        return true;
      }
      identityInput.addClass("content-border-danger");
      let invalidMessage = $("#invalidEmailOrPhoneNumber").attr("data-value")
      addInvalidMessage(invalidMessage);
    }
    else {

      let emptyMessage = $('div.validation-message-text[data-field="Identity"][data-validate="data-val-required"]').text();
      addInvalidMessage(emptyMessage);
    }

    return false;
  }
  function addInvalidMessage(message) {
    const $parentSpan = $('span[data-valmsg-for="Identity"]');

    // Tìm span con #identity-error bên trong
    let $errorSpan = $parentSpan.find("#identity-error");

    if ($errorSpan.length > 0) {
      // Nếu đã có thì đổi nội dung
      $errorSpan.text(message);
    } else {
      // Nếu chưa có thì thêm span con vào
      $parentSpan.append('<span id="identity-error">' + message + '</span>');
    }
  }


  function isNullOrWhiteSpace(input) {
    return !((input ?? "").trim());
  }
  function validateAll() {
    let isDoneAllValidate = true;

    const isValidEmail = validateEmail();
    if (!isValidEmail) isDoneAllValidate = false;

    return isDoneAllValidate;
  }

}(jQuery));	

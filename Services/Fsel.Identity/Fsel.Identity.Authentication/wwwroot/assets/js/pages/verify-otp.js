(function ($) {
  $('#form-VerifyOtp').on('submit', function (e) {
    if (this.checkValidity()) {
      $('.loading').removeClass('hidden');
    }
    else {
      e.preventDefault();
    }
  });

  $('#zalo').click(function () {
    $('#OtpProvider').val("Zalo");
  });

  $('#sms').click(function () {
    $('#OtpProvider').val("Sms");
  });

  $('#email').click(function () {
    $('#OtpProvider').val("Email");
  });

  const currentUrl = new URL(window.location.href);
  currentUrl.searchParams.delete('otpInfo');
  window.history.replaceState({}, document.title, currentUrl.toString());

  $(".number-item").keyup(function (event) {
    var currentInput = $(this);
    var index = $(".number-item").index(currentInput);
    var previousInputId = index > 0 ? "otp-" + index : null;
    var nextInputId = index < $(".number-item").length - 1 ? "otp-" + (index + 2) : null;

    moveToNext(event, currentInput, previousInputId, nextInputId);
    valueFill();
  });

  $(".number-item").on("paste", function (event) {
    var currentInput = $(this);
    autoFill(event, currentInput);
    valueFill();
  });

  function valueFill() {
    var otpValue = "";
    $(".number-item").each(function () {
      otpValue += $(this).val();
    });
    $("#otp").val(otpValue);
    $(".message-otp-error").remove();
    $(".number-list").removeClass("number-list-error");
  }

  $.validator.methods.required = function (value, element, param) {
    if (typeof value === "string") {
      value = $.trim(value);
    }
    return value.length > 0;
  }

  let remainSeconds = parseInt($("#RemainSecond").attr("data-value"));
  let countTimeInSecond = $("#countTimeInSecond").attr("data-value")
  let countTimeInMinutesSecond = $("#countTimeInMinutesSecond").attr("data-value")
  let isLocked = $("#isBlocked").attr("data-value") === 'true'
  let isLockedResendOtp = $("#isLockedResendOtp").attr("data-value").toLowerCase() === 'true'
  let maxNumberOfVerify = $("#maxNumberOfVerify").attr("data-value")
  let startTime = Date.now();
  function formatString(template, ...values) {
    return template.replace(/\{\{(\d+)\}\}/g, (match, index) => {
      const idx = parseInt(index);
      return idx < values.length ? values[idx] : match;
    });
  }
  function updateCountdown() {
    let elapsedTime = Math.floor((Date.now() - startTime) / 1000);
    let currentSeconds = remainSeconds - elapsedTime;

    if (currentSeconds > 0) {
      let displayText;
      if (currentSeconds > 60) {
        const minutes = Math.floor(currentSeconds / 60);
        const seconds = currentSeconds % 60;
        if (isLocked) {
          displayText = formatString(countTimeInMinutesSecond, maxNumberOfVerify, minutes);
        }
        else {
          displayText = formatString(countTimeInMinutesSecond, minutes, seconds);
        }
      }
      else {
        if (isLocked) {
          displayText = formatString(countTimeInMinutesSecond, maxNumberOfVerify, "1");
        } else {
          displayText = formatString(countTimeInSecond, currentSeconds);
        }
      }

      const text = "<p>" + displayText + "</p>";
      if (isLocked) {
        $(".resend-otp-1").html(text);
        $(".resend-otp-1").removeClass("hidden");
        $(".resend-otp-2").addClass("hidden");
        $(".resend-otp-3").addClass("hidden");
      }
      else if (isLockedResendOtp) {
        $(".resend-otp-1").addClass("hidden");
        $(".resend-otp-2").removeClass("hidden");
        $(".resend-otp-3").removeClass("hidden");
        $("#sms").prop("disabled", false);
      }
      else {
        $(".resend-otp-1").html(text);
        $(".resend-otp-1").removeClass("hidden");
        $(".resend-otp-2").addClass("hidden");
      }
      timeoutId = setTimeout(updateCountdown, 1000);
    } else {
      $(".resend-otp-2").removeClass("hidden");
      $(".resend-otp-3").removeClass("hidden");
      $(".resend-otp-1").addClass("hidden");
      $("#sms").prop("disabled", false);
      clearTimeout(timeoutId);
    }
  }
  function moveToNext(event, currentInput, previousInputId, nextInputId) {
    var maxLength = parseInt(currentInput.attr("maxlength"));
    var currentLength = currentInput.val().length;
    var key = event.originalEvent.key || String.fromCharCode(event.which || event.originalEvent.keyCode);

    if (key === "Backspace" && previousInputId) {
      var previousInput = $("#" + previousInputId);
      if (previousInput.length) {
        setTimeout(function () {
          previousInput.val(""); // Xóa giá trị của ô OTP trước đó
          previousInput.focus();
        }, 0);
      }
    } else if ((key !== "Backspace" && currentLength >= maxLength) || (key === "Backspace" && !previousInputId)) {
      setTimeout(function () {
        var nextInput = $("#" + nextInputId);
        if (nextInput.length && currentInput.val() !== "") {
          nextInput.focus();
        }
      }, 0);
    }
  }

  function autoFill(event, currentInput) {
    var pastedData = event.originalEvent.clipboardData.getData("text");
    var otps = pastedData.split("");
    var shouldAutoFill = false;
    var otpInputs = $(".number-item");

    otpInputs.each(function (index) {
      var otpInput = $(this);

      if (otpInput[0] === currentInput[0]) {
        shouldAutoFill = true;
      }

      if (shouldAutoFill && otpInput.val() === "") {
        if (otps.length > 0) {
          otpInput.val(otps.shift());
          console.log("OTP-" + (index + 1) + ": " + otpInput.val());
        } else {
          return false; // Dừng vòng lặp nếu đã hết giá trị để điền
        }
      }
      otpInput.blur(); // Bỏ focus khỏi ô OTP
    });

    event.preventDefault(); // Ngăn không cho việc dán mặc định xảy ra
  }
  function countdownStep() {
    updateCountdown(); // Cập nhật thời gian mỗi giây
    timeoutId = setTimeout(countdownStep, 1000); // Lặp lại sau mỗi 1 giây
  }

  let timeoutId = setTimeout(countdownStep, 1000);
}(jQuery));

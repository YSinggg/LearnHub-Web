// 1. Form checking

// Find the red error text 
function showError(input, message) {
  var span = input.form.querySelector('[data-valmsg-for="' + input.name + '"]');

  if (message == "") {
    input.classList.remove("input-validation-error");
  } else {
    input.classList.add("input-validation-error");
  }

  if (span != null) {
    span.textContent = message;
    if (message == "") {
      span.className = "field-validation-valid";
    } else {
      span.className = "field-validation-error";
    }
  }
}

// Password must have lowercase, uppercase, number and symbol
function isStrongPassword(password) {
  var hasLower = /[a-z]/.test(password);
  var hasUpper = /[A-Z]/.test(password);
  var hasNumber = /[0-9]/.test(password);
  var hasSymbol = /[^a-zA-Z0-9]/.test(password);
  return hasLower && hasUpper && hasNumber && hasSymbol;
}

// The "confirm password" box has "Confirm" in its name
function isConfirmBox(input) {
  return input.name.indexOf("Confirm") != -1 && input.type == "password";
}

// Find the new password box in the same form
function findNewPassword(form) {
  var boxes = form.querySelectorAll('input[type="password"]');
  for (var i = 0; i < boxes.length; i++) {
    if (boxes[i].autocomplete == "new-password" && !isConfirmBox(boxes[i])) {
      return boxes[i];
    }
  }
  return null;
}

// Check one input. Returns the error message
function checkInput(input) {
  var value = input.value;

  // Required fields
  if (input.required) {
    var customMessage = input.getAttribute("data-msg-required");

    if (input.type == "checkbox" && input.checked == false) {
      if (customMessage != null) return customMessage;
      return "Please tick this box.";
    }
    if (input.type != "checkbox" && value.trim() == "") {
      if (customMessage != null) return customMessage;
      return "This field is required.";
    }
  }

  if (value == "") return "";

  // Email
  if (input.type == "email" && value.indexOf("@") == -1) {
    return "Enter a valid email address.";
  }

  // Minimum length
  if (input.minLength > 0 && value.length < input.minLength) {
    return "Must be at least " + input.minLength + " characters.";
  }

  // New password must be strong
  if (input.autocomplete == "new-password" && !isConfirmBox(input)) {
    if (!isStrongPassword(value)) {
      return "Use uppercase, lowercase, a number and a symbol.";
    }
  }

  // Confirm password must match
  if (isConfirmBox(input)) {
    var newPassword = findNewPassword(input.form);
    if (newPassword != null && newPassword.value != value) {
      return "Passwords do not match.";
    }
  }

  // Anything else the browser knows (example min and max numbers)
  if (!input.checkValidity()) {
    return input.validationMessage;
  }

  return "";
}

// Get the inputs in a form that the user can type in
function getInputs(form) {
  var result = [];
  var all = form.querySelectorAll("input, select, textarea");
  for (var i = 0; i < all.length; i++) {
    var type = all[i].type;
    if (type != "hidden" && type != "submit" && type != "button") {
      result.push(all[i]);
    }
  }
  return result;
}

var forms = document.querySelectorAll("form");

for (var f = 0; f < forms.length; f++) {
  var form = forms[f];

  // Turn off the browser's popup to show messages
  form.noValidate = true;

  // Check a box when the user leaves it
  var inputs = getInputs(form);
  for (var i = 0; i < inputs.length; i++) {
    inputs[i].addEventListener("blur", function () {
      showError(this, checkInput(this));
    });
  }

  // Check every box when the form is submitted
  form.addEventListener("submit", function (event) {
    var boxes = getInputs(this);
    var firstWrong = null;

    for (var i = 0; i < boxes.length; i++) {
      var message = checkInput(boxes[i]);
      showError(boxes[i], message);
      if (message != "" && firstWrong == null) {
        firstWrong = boxes[i];
      }
    }

    // Something is wrong, so stop the form and go to the first wrong box
    if (firstWrong != null) {
      event.preventDefault();
      firstWrong.focus();
      return;
    }

    // Delete buttons ask "are you sure?" first
    var question = this.getAttribute("data-confirm");
    if (question != null) {
      if (!confirm(question)) {
        event.preventDefault();
      }
    }
  });
}


// 2. Show / hide password and strength

function getStrengthText(password) {
  var score = 0;
  if (password.length >= 10) score++;
  if (/[a-z]/.test(password)) score++;
  if (/[A-Z]/.test(password)) score++;
  if (/[0-9]/.test(password)) score++;
  if (/[^a-zA-Z0-9]/.test(password)) score++;

  var word = "Very weak";
  if (score == 2) word = "Weak";
  if (score == 3) word = "Fair";
  if (score == 4) word = "Good";
  if (score == 5) word = "Strong";

  return "Password strength: " + word + " (" + score + " of 5)";
}

// Show security notice when copy or paste is attempted on password fields
function showPasswordSecurityNotice(box, actionText) {
  var notice = box.securityNotice;
  if (!notice) {
    notice = document.createElement("div");
    notice.className = "password-security-notice";

    var parent = box.parentNode;
    var next = box.nextElementSibling;
    while (next && (next.classList.contains("password-toggle") || next.classList.contains("strength"))) {
      next = next.nextElementSibling;
    }
    if (next) {
      parent.insertBefore(notice, next);
    } else {
      parent.appendChild(notice);
    }
    box.securityNotice = notice;
  }

  notice.innerHTML = "🔒 " + actionText + " is disabled on password fields.";
  notice.style.display = "block";
  clearTimeout(notice.timer);
  notice.timer = setTimeout(function () {
    notice.style.display = "none";
  }, 3500);
}

var passwordBoxes = document.querySelectorAll('input[type="password"]');

for (var p = 0; p < passwordBoxes.length; p++) {
  var box = passwordBoxes[p];

  // Disable copy, cut, paste, drop, and drag on password fields (safety rule)
  box.addEventListener("copy", function (e) {
    e.preventDefault();
    showPasswordSecurityNotice(this, "Copying");
  });
  box.addEventListener("cut", function (e) {
    e.preventDefault();
    showPasswordSecurityNotice(this, "Cutting");
  });
  box.addEventListener("paste", function (e) {
    e.preventDefault();
    showPasswordSecurityNotice(this, "Pasting");
  });
  box.addEventListener("drop", function (e) {
    e.preventDefault();
  });
  box.addEventListener("dragstart", function (e) {
    e.preventDefault();
  });

  // Show / hide button
  var showButton = document.createElement("button");
  showButton.type = "button";
  showButton.className = "quiet password-toggle";
  showButton.textContent = "Show password";
  showButton.passwordBox = box;

  showButton.addEventListener("click", function () {
    if (this.passwordBox.type == "password") {
      this.passwordBox.type = "text";
      this.textContent = "Hide password";
    } else {
      this.passwordBox.type = "password";
      this.textContent = "Show password";
    }
  });

  box.after(showButton);

  // Strength text only for the new password box
  if (box.autocomplete == "new-password" && !isConfirmBox(box)) {
    var strength = document.createElement("small");
    strength.className = "strength";
    strength.textContent = "Password strength: not entered";
    showButton.after(strength);
    box.strengthText = strength;

    box.addEventListener("input", function () {
      if (this.value == "") {
        this.strengthText.textContent = "Password strength: not entered";
      } else {
        this.strengthText.textContent = getStrengthText(this.value);
      }
    });
  }
}


// 3. Filter courses on the home page
var searchBox = document.getElementById("S");
var categoryBox = document.getElementById("Category");

function filterCourses() {
  var searchText = searchBox.value.trim().toLowerCase();
  var category = categoryBox.value;
  var cards = document.querySelectorAll("[data-course]");
  var shown = 0;

  for (var i = 0; i < cards.length; i++) {
    var courseText = cards[i].getAttribute("data-course").toLowerCase();
    var courseCategory = cards[i].getAttribute("data-category");

    var nameOk = searchText == "" || courseText.indexOf(searchText) != -1;
    var categoryOk = category == "" || courseCategory == category;

    if (nameOk && categoryOk) {
      cards[i].style.display = "";
      shown++;
    } else {
      cards[i].style.display = "none";
    }
  }

  var countText = document.getElementById("course-count");
  if (countText != null) {
    if (shown == 1) {
      countText.textContent = "1 matching course";
    } else {
      countText.textContent = shown + " matching courses";
    }
  }
}

if (categoryBox != null) {
  categoryBox.addEventListener("change", filterCourses);
}


// 4. Quiz progress, auto-save draft with date/time, and submission validation

var quiz = document.querySelector(".quiz");

function getQuizStorageKey() {
  if (!quiz) return "";
  var courseId = quiz.getAttribute("data-course-id") || "0";
  var userId = quiz.getAttribute("data-user-id") || "0";
  return "learnhub_quiz_draft_course_" + courseId + "_user_" + userId;
}

function updateQuizCount() {
  if (!quiz) return;
  var total = quiz.querySelectorAll("fieldset").length;
  var answered = quiz.querySelectorAll('input[type="radio"]:checked').length;

  var countElem = document.getElementById("answer-count");
  if (countElem) {
    countElem.textContent = answered + " of " + total + " answered";
  }
  var progressElem = document.getElementById("answer-progress");
  if (progressElem) {
    progressElem.value = answered;
  }
}

// Store date and time and currently selected options whenever user clicks
function saveQuizDraft() {
  var key = getQuizStorageKey();
  if (!key || !quiz) return;

  var answers = {};
  var checkedRadios = quiz.querySelectorAll('input[type="radio"]:checked');
  checkedRadios.forEach(function (radio) {
    answers[radio.name] = radio.value;
  });

  var courseId = quiz.getAttribute("data-course-id") || "0";
  var courseTitle = quiz.getAttribute("data-course-title") || "";
  var total = quiz.querySelectorAll("fieldset").length;
  var answered = checkedRadios.length;

  var now = new Date();
  var dateStr = now.toLocaleDateString();
  var timeStr = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: true });
  var fullTimeStr = dateStr + " " + timeStr;

  var draftData = {
    courseId: courseId,
    courseTitle: courseTitle,
    answered: answered,
    total: total,
    savedAt: fullTimeStr,
    savedDate: dateStr,
    savedTime: timeStr,
    timestamp: now.getTime(),
    answers: answers
  };

  try {
    localStorage.setItem(key, JSON.stringify(draftData));
    var indicator = document.getElementById("auto-save-indicator");
    if (indicator) {
      indicator.textContent = "💾 Auto-saved at: " + fullTimeStr;
    }
  } catch (err) {
    // LocalStorage quota or access error
  }
}

// Restore draft on page load so user stays where they left off
function restoreQuizDraft() {
  var key = getQuizStorageKey();
  if (!key || !quiz) return;

  try {
    var raw = localStorage.getItem(key);
    if (!raw) return;

    var draft = JSON.parse(raw);
    if (!draft || !draft.answers) return;

    var restoredCount = 0;
    for (var name in draft.answers) {
      var val = draft.answers[name];
      var radio = quiz.querySelector('input[type="radio"][name="' + CSS.escape(name) + '"][value="' + CSS.escape(val) + '"]');
      if (radio) {
        radio.checked = true;
        restoredCount++;
      }
    }

    if (restoredCount > 0) {
      var banner = document.getElementById("quiz-draft-banner");
      var timeDisplay = document.getElementById("draft-saved-time");
      if (banner && timeDisplay) {
        timeDisplay.textContent = draft.savedAt || "Previously";
        banner.style.display = "flex";
      }
      var indicator = document.getElementById("auto-save-indicator");
      if (indicator) {
        indicator.textContent = "💾 Restored from draft (" + (draft.savedAt || "Previously") + ")";
      }

      // Enrich old drafts if courseTitle or total were missing
      if (!draft.courseTitle || !draft.total) {
        draft.courseId = quiz.getAttribute("data-course-id") || "0";
        draft.courseTitle = quiz.getAttribute("data-course-title") || "";
        draft.answered = restoredCount;
        draft.total = quiz.querySelectorAll("fieldset").length;
        if (!draft.savedDate && draft.savedAt) {
          var parts = draft.savedAt.trim().split(" ");
          draft.savedDate = parts[0] || "";
          draft.savedTime = parts.slice(1).join(" ") || "";
        }
        try { localStorage.setItem(key, JSON.stringify(draft)); } catch (e) {}
      }
    }
  } catch (err) {
    // Ignore parse error
  }
}

function clearQuizDraft() {
  var key = getQuizStorageKey();
  if (key) {
    localStorage.removeItem(key);
  }
  if (quiz) {
    quiz.querySelectorAll('input[type="radio"]').forEach(function (r) {
      r.checked = false;
    });
    updateQuizCount();
  }
  var banner = document.getElementById("quiz-draft-banner");
  if (banner) banner.style.display = "none";
  var indicator = document.getElementById("auto-save-indicator");
  if (indicator) indicator.textContent = "Draft reset.";
  var alertBox = document.getElementById("quiz-validation-alert");
  if (alertBox) alertBox.style.display = "none";
}

if (quiz != null) {
  // Restore any previously saved draft
  restoreQuizDraft();
  updateQuizCount();

  // Listen to option changes: update count, clear highlight, and auto-save
  quiz.addEventListener("change", function (e) {
    updateQuizCount();
    saveQuizDraft();

    // If an answered question was previously highlighted, remove warning
    if (e.target && e.target.type === "radio") {
      var fieldset = e.target.closest("fieldset");
      if (fieldset) {
        fieldset.classList.remove("unanswered-question");
        var badge = fieldset.querySelector(".unanswered-badge");
        if (badge) badge.style.display = "none";
      }
      var alertBox = document.getElementById("quiz-validation-alert");
      var remaining = quiz.querySelectorAll("fieldset.unanswered-question").length;
      if (alertBox && remaining === 0) {
        alertBox.style.display = "none";
      }
    }
  });

  // Validate on submit: alert if any question is unanswered & highlight it
  quiz.addEventListener("submit", function (e) {
    var fieldsets = quiz.querySelectorAll("fieldset");
    var unanswered = [];

    fieldsets.forEach(function (fs) {
      var checked = fs.querySelector('input[type="radio"]:checked');
      var badge = fs.querySelector(".unanswered-badge");
      if (!checked) {
        unanswered.push(fs);
        fs.classList.add("unanswered-question");
        if (badge) badge.style.display = "inline";
      } else {
        fs.classList.remove("unanswered-question");
        if (badge) badge.style.display = "none";
      }
    });

    if (unanswered.length > 0) {
      e.preventDefault();
      var alertBox = document.getElementById("quiz-validation-alert");
      if (alertBox) {
        alertBox.style.display = "block";
      }
      // Smoothly scroll to the first unanswered question
      unanswered[0].scrollIntoView({ behavior: "smooth", block: "center" });
      return false;
    }

    // All answered: clear draft so retaking starts clean
    var key = getQuizStorageKey();
    if (key) {
      localStorage.removeItem(key);
    }
  });
}


// 5. YouTube video preview (admin lesson page)
var videoBox = document.getElementById("Input_VideoUrl");
var videoPreview = document.getElementById("video-preview");

// Get the 11 letter video id from a YouTube link, or "" if not found
function getYouTubeId(link) {
  var markers = ["v=", "youtu.be/", "embed/"];
  for (var i = 0; i < markers.length; i++) {
    var place = link.indexOf(markers[i]);
    if (place != -1) {
      var id = link.substr(place + markers[i].length, 11);
      if (id.length == 11) return id;
    }
  }
  return "";
}

function showVideoPreview() {
  var link = videoBox.value.trim();
  videoPreview.innerHTML = "";

  if (link == "") return;

  var id = getYouTubeId(link);
  if (id == "") {
    var error = document.createElement("small");
    error.className = "field-validation-error";
    error.textContent = "This is not a YouTube link.";
    videoPreview.appendChild(error);
  } else {
    var frame = document.createElement("iframe");
    frame.className = "lesson-video";
    frame.title = "Video preview";
    frame.src = "https://www.youtube-nocookie.com/embed/" + id;
    videoPreview.appendChild(frame);
  }
}

if (videoBox != null && videoPreview != null) {
  videoBox.addEventListener("input", showVideoPreview);
  showVideoPreview();
}


// 6. Code playground

var runButton = document.getElementById("run-code");

if (runButton != null) {
  runButton.addEventListener("click", function () {
    var code = document.getElementById("playground").value;
    // The iframe has sandbox="allow-scripts" so the code cannot touch our site
    document.getElementById("code-preview").srcdoc = code;
  });
}

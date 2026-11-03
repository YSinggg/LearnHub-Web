// ---------- 1. Form checking ----------

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


// ---------- 2. Show / hide password and strength ----------

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

var passwordBoxes = document.querySelectorAll('input[type="password"]');

for (var p = 0; p < passwordBoxes.length; p++) {
  var box = passwordBoxes[p];

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


// ---------- 3. Filter courses on the home page ----------

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
    if (shown == 1) {
      countText.textContent = "1 matching course";
    } else {
      countText.textContent = shown + " matching courses";
    }
  
}

if (categoryBox != null) {
  categoryBox.addEventListener("change", filterCourses);
}


// ---------- 4. Count answered quiz questions ----------

var quiz = document.querySelector(".quiz");

function updateQuizCount() {
  var total = quiz.querySelectorAll("fieldset").length;
  // Each question can only have one ticked answer
  var answered = quiz.querySelectorAll('input[type="radio"]:checked').length;

  document.getElementById("answer-count").textContent = answered + " of " + total + " answered";
  document.getElementById("answer-progress").value = answered;
}

if (quiz != null) {
  quiz.addEventListener("change", updateQuizCount);
  updateQuizCount();
}


// ---------- 5. YouTube video preview (admin lesson page) ----------

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


// ---------- 6. Code playground ----------

var runButton = document.getElementById("run-code");

if (runButton != null) {
  runButton.addEventListener("click", function () {
    var code = document.getElementById("playground").value;
    // The iframe has sandbox="allow-scripts" so the code cannot touch our site
    document.getElementById("code-preview").srcdoc = code;
  });
}
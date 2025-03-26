import { startAuthentication, startRegistration } from "@simplewebauthn/browser";

const signupButton = document.querySelector("[data-signup]");
const loginButton = document.querySelector("[data-login]");
const emailInput = document.querySelector("[data-email]");
const modal = document.querySelector("[data-modal]");
const closeButton = document.querySelector("[data-close]");
const timerDisplay = document.createElement("div");

timerDisplay.style.marginTop = "1rem";
timerDisplay.style.fontSize = "1.2rem";
timerDisplay.style.color = "red";
document.body.appendChild(timerDisplay);

const alertSound = new Audio("/audio/alert.mp3"); // Example alert sound URL
alertSound.loop = true; // Looping the sound during alert duration
let alertTimeout;

const SERVER_URL = "http://localhost:3000";
const MAX_LOGIN_ATTEMPTS = 3;
const LOCKOUT_TIME = 2 * 60 * 1000; // 2 minutes in milliseconds
let loginAttempts = 0;

signupButton.addEventListener("click", signup);
loginButton.addEventListener("click", login);
closeButton.addEventListener("click", () => modal.close());

function updateLockoutTimer() {
  const lockoutUntil = localStorage.getItem("lockoutUntil");

  if (lockoutUntil && Date.now() < parseInt(lockoutUntil)) {
    const remainingTime = Math.ceil((parseInt(lockoutUntil) - Date.now()) / 1000);
    timerDisplay.innerText = `Locked out. Try again in ${remainingTime} seconds.`;

    // If remaining time is exactly 30 seconds, start alert sound
    if (remainingTime === 30 ) {
      alertSound.play();
      alertTimeout = setTimeout(() => {
        alertSound.pause();
        alertSound.currentTime = 0;
      }, 10000); // Stop the alert after 10 seconds
    }

    setTimeout(updateLockoutTimer, 1000);
  } else {
    timerDisplay.innerText = "";
    alertSound.pause();
    alertSound.currentTime = 0;
    clearTimeout(alertTimeout);
  }
}

async function signup() {
  const email = emailInput.value.trim();

  if (email !== "ritadehrawala3@gmail.com") {
    showModalText("Something went wrong. Please try again.");
    return;
  }

  try {
    const initResponse = await fetch(`${SERVER_URL}/init-register?email=${email}`, {
      credentials: "include",
    });
    const options = await initResponse.json();
    if (!initResponse.ok) {
      showModalText(options.error);
      return;
    }

    const registrationJSON = await startRegistration(options);

    const verifyResponse = await fetch(`${SERVER_URL}/verify-register`, {
      credentials: "include",
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(registrationJSON),
    });

    const verifyData = await verifyResponse.json();
    if (!verifyResponse.ok) {
      showModalText(verifyData.error);
      return;
    }

    if (verifyData.verified) {
      showModalText(`Successfully registered ${email}`);
      savePasskey(email, verifyData.passkey);
    } else {
      showModalText("Failed to register.");
    }
  } catch (error) {
    showModalText("An error occurred during signup.");
  }
}


async function login() {
  const lockoutUntil = localStorage.getItem("lockoutUntil");
  if (lockoutUntil && Date.now() < parseInt(lockoutUntil)) {
    updateLockoutTimer();
    return;
  }

  const email = emailInput.value.trim();

  if (!email) {
    showModalText("Please enter a valid email.");
    return;
  }

  try {
    const initResponse = await fetch(`${SERVER_URL}/init-auth?email=${email}`, {
      credentials: "include",
    });
    const options = await initResponse.json();
    if (!initResponse.ok) {
      handleFailedLogin(options.error);
      return;
    }

    const authJSON = await startAuthentication(options);

    const verifyResponse = await fetch(`${SERVER_URL}/verify-auth`, {
      credentials: "include",
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(authJSON),
    });

    const verifyData = await verifyResponse.json();
    if (!verifyResponse.ok) {
      handleFailedLogin(verifyData.error);
      return;
    }

    if (verifyData.verified) {
      showModalText(`Successfully logged in ${email}`);
      savePasskey(email, verifyData.passkey);
      loginAttempts = 0;
      localStorage.removeItem("lockoutUntil");

      setTimeout(() => {
        if (email === "ritadehrawala3@gmail.com") {
          window.location.href = "http://localhost:5285/Admin/Index";
        } else {
          showModalText("Proceeding with WebAuthn authentication...");
          window.location.href = "http://localhost:5285";
        }
      }, 1000);
    } else {
      handleFailedLogin("Failed to log in.");
    }
  } catch (error) {
    handleFailedLogin("An error occurred during login.");
  }
}

function handleFailedLogin(errorMessage) {
  loginAttempts++;
  showModalText(`Login attempt ${loginAttempts} failed: ${errorMessage}`);

  if (loginAttempts >= MAX_LOGIN_ATTEMPTS) {
    const lockoutUntil = Date.now() + LOCKOUT_TIME;
    localStorage.setItem("lockoutUntil", lockoutUntil);
    showModalText("Too many failed login attempts. Please try again later.");
    updateLockoutTimer();
  }
}

function showModalText(text) {
  modal.querySelector("[data-content]").innerText = text;
  modal.showModal();
}

function savePasskey(email, passkey) {
  const expirationTime = Date.now() + 365 * 24 * 60 * 60 * 1000;
  const data = { passkey, expires: expirationTime };
  localStorage.setItem(`passkey_${email}`, JSON.stringify(data));
}

function getPasskey(email) {
  const data = JSON.parse(localStorage.getItem(`passkey_${email}`));
  if (data && Date.now() < data.expires) {
    return data.passkey;
  } else {
    localStorage.removeItem(`passkey_${email}`);
    return null;
  }
}

updateLockoutTimer();

const API_BASE = window.location.origin  + "/api"; // adjust port

let goals = [];
let currentUser = null;
document.addEventListener("DOMContentLoaded", async () => {
    updateUIState();

    if (!isLoggedIn()) return;

    // ✅ only run if elements exist
    if (document.getElementById("goalsTable")) {
        await loadCurrentUser();
        await loadGoals();
        renderGoals();
    }
});



function logout() {
    localStorage.removeItem("token");
    toast("Logged out");
    updateUIState();
}


// ✅ Load Goals
async function loadGoals() {
    const res = await fetch(`${API_BASE}/goals`,
        {
            headers: getAuthHeaders()
        }
    );

    if (!res.ok) {
        goals = [];
        return;
    }

    goals = await res.json();
}


// ✅ Render Goals table
function renderGoals() {
    const tbody = document.getElementById("goalsTable");

    tbody.innerHTML = "";

    const filtered = goals.filter(g => g.userId === currentUser?.id);

    filtered.forEach(goal => {
        const tr = document.createElement("tr");

        // ✅ Styling rules
        const now = new Date();
        const due = new Date(goal.dueDate);

        if (goal.status === 2) { // Completed
            tr.classList.add("completed");
        } else if (due < now) {
            tr.classList.add("overdue");
        }

        const assessment = assessGoal(goal);

        tr.innerHTML = `
            <td>${goal.title}</td>
            <td>${getStatusText(goal.status)}</td>
            <td>${goal.points}</td>
            <td>
                <input type="number" min="0" max="100" 
                    value="${goal.progressPercent}" 
                    id="progress-${goal.id}" 
                    class="form-control form-control-sm" />

                    <div class="progress mt-1">
                        <div class="progress-bar" style="width: ${goal.progressPercent}%"></div>
                    </div>

            </td>
            <td>${new Date(goal.dueDate).toLocaleDateString()}</td>
            <td>
                <button class="btn btn-sm btn-primary" 
                    onclick="updateProgress('${goal.id}')">
                    Save
                </button>
            </td>
            <td class="${assessment.class}">
                ${assessment.text}
            </td>
            <td>
                <button class="btn btn-sm btn-danger" 
                    onclick="deleteGoal('${goal.id}')">
                    🗑️
                </button>
            </td>
`;


        tbody.appendChild(tr);
    });
}

async function updateProgress(goalId) {
    const input = document.getElementById(`progress-${goalId}`);
    const newProgress = parseInt(input.value);

    const goal = goals.find(g => g.id === goalId);

    // ✅ update locally
    goal.progressPercent = newProgress;

    // ✅ auto status rule (simple MVP logic)
    if (newProgress === 100) {
        goal.status = 2;
    } else if (newProgress > 0) {
        goal.status = 1;
    }

    // ✅ send to API
    var res = await fetch(`${API_BASE}/goals/${goalId}/progress`, {
        method: "PATCH",
        headers: getAuthHeaders(),       
        body: JSON.stringify({
            progressPercent: newProgress
        })
    });
    

    if (res.ok) {
        newProgress == 100 ? toast("Goal completed! 🎉", "success") : toast("✅ Progress updated");
        await loadGoals();
        renderGoals();
    } else {
        toast("❌ Failed to update");
    }
}
async function createGoal() {
    const title = document.getElementById("title").value;
    const description = document.getElementById("description").value;
    const points = parseInt(document.getElementById("points").value);
    const dueDate = document.getElementById("dueDate").value;

    const errorDiv = document.getElementById("createError");
    errorDiv.innerText = "";

    // ✅ Basic validation
    if (!title) {
        errorDiv.innerText = "Title is required";
        return;
    }

    if (!dueDate) {
        errorDiv.innerText = "Due date is required";
        return;
    }

    if (points <= 0 || isNaN(points)) {
        errorDiv.innerText = "Points must be greater than 0";
        return;
    }

    // ✅ Build payload
    const newGoal = {
        title: title,
        description: description,
        points: points,
        dueDate: dueDate,
        status: 0, // NotStarted
        progressPercent: 0
    };

    const res = await fetch(`${API_BASE}/goals`, {
        method: "POST",
        headers: getAuthHeaders(),
        body: JSON.stringify(newGoal)
    });

    if (res.ok) {
        toast("✅ Goal created");

        // ✅ Reset form
        document.getElementById("title").value = "";
        document.getElementById("description").value = "";
        document.getElementById("points").value = "";
        document.getElementById("dueDate").value = "";

        await loadGoals();
        renderGoals();
    } else {
        errorDiv.innerText = "Failed to create goal";
    }
}


// ✅ Helper
function getStatusText(status) {
    switch (status) {
        case 0: return "Not Started";
        case 1: return "In Progress";
        case 2: return "Completed";
        default: return "Unknown";
    }
}

// ── Toast helper ──────────────────────────────────────────────
function toast(message, type = 'error') {
    Toastify({
        text: message,
        duration: 4000,
        gravity: 'top',
        position: 'center',
        stopOnFocus: true,
        style: {
            background: type === 'success'
                ? 'linear-gradient(to right, #0F9D58, #34a853)'
                : 'linear-gradient(to right, #d40000, #b00020)',
            borderRadius: '8px',
            fontSize: '0.95rem',
            padding: '12px 20px',
            boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
        }
    }).showToast();
}
function assessGoal(goal) {
    const now = new Date();
    const due = new Date(goal.dueDate);

    if (goal.status === 2) {
        if (due > now) {
            return { text: "✅ Reward", class: "reward" };
        } else {
            return { text: "⚠️ Late", class: "late" };
        }
    }

    if (due < now && goal.progressPercent < 100) {
        return { text: "❌ Punishment", class: "punishment" };
    }

    return { text: "🟡 On Track", class: "ontrack" };
}
async function deleteGoal(goalId) {

    // ✅ confirm before deleting
    if (!confirm("Are you sure you want to delete this goal?")) {
        return;
    }

    const res = await fetch(`${API_BASE}/goals/${goalId}`, {
        method: "DELETE",
        headers: getAuthHeaders()
    });

    if (res.ok) {
        toast("Goal deleted 🗑️", "success");

        await loadGoals();
        renderGoals();
    } else {
        toast("Failed to delete ❌", "error");
    }
}
async function evaluateAll() {
    await fetch(`${API_BASE}/goals/evaluate-all`, {
        method: "POST",
        headers: getAuthHeaders()
    });

    toast("Evaluation triggered ⚙️", "warning");

    await loadGoals();
    renderGoals();
}
async function login() {
    const email = document.getElementById("loginEmail").value;
    const password = document.getElementById("loginPassword").value;


    const res = await fetch(`${API_BASE}/users/login`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ email, password })
    });


    if (!res.ok) {
        toast("Login failed ❌", "error");
        return;
    }

    const data = await res.json();

    localStorage.setItem("token", data.token);

    
    updateUIState();
    await loadCurrentUser();
    await loadGoals();
    renderGoals();
    toast("Logged in ✅", "success");
}

function getAuthHeaders() {
    const token = localStorage.getItem("token");

    return {
        "Content-Type": "application/json",
        "Authorization": "Bearer " + token
    };
}

async function loadCurrentUser() {
    const res = await fetch(`${API_BASE}/users/me`, {
        headers: getAuthHeaders()
    });

    if (res.ok) {
        currentUser = await res.json();

        document.getElementById("currentUserInfo").innerText =
            `Logged in as: ${currentUser.email}`;

        document.getElementById("userPoints").innerText =
            `Points: ${currentUser.totalPoints}`;
    }
    else {
        currentUser = null;
        return;
    }
}


function isLoggedIn() {
    return localStorage.getItem("token") !== null;
}

function updateUIState() {
    const loggedIn = isLoggedIn();

    const loginSection = document.getElementById("loginSection");
    const logoutSection = document.getElementById("logoutSection");
    const appSection = document.getElementById("appSection");

    if (loginSection) {
        loginSection.style.display = loggedIn ? "none" : "block";
    }

    if (logoutSection) {
        logoutSection.style.display = loggedIn ? "block" : "none";
    }

    if (appSection) {
        appSection.style.display = loggedIn ? "block" : "none";
    }
}
async function register() {
    const email = document.getElementById("regEmail").value;
    const password = document.getElementById("regPassword").value;
    const phone = document.getElementById("regPhone").value;

    const res = await fetch(`${API_BASE}/users/register`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            email,
            password,
            phoneNumber: phone
        })
    });
    let message = "Registration failed ❌";

    if (!res.ok) {
        const text = await res.text(); // ✅ always safe

        try {
            const errors = JSON.parse(text);

            if (Array.isArray(errors)) {
                message = errors[0]?.description || message;
            } else {
                message = text;
            }
        } catch {
            // ✅ not JSON → plain string
            message = text;
        }

        toast(message, "error");
        return;
    }
    

    // ✅ success
    data = await res.json();
    toast("Registered ✅");


    // ✅ redirect to login
    window.location.href = "/";
}





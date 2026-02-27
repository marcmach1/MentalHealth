async function loadProfessionals() {
  const container = document.getElementById("professionals-list");
  if (!container) return;

  try {
    const response = await fetch("/api/ProfessionalsApi");
    if (!response.ok) throw new Error("Erro ao carregar profissionais");
    const professionals = await response.json();

    if (!Array.isArray(professionals) || professionals.length === 0) {
      container.innerHTML = '<div class="col-12 text-muted">Ainda não há profissionais cadastrados.</div>';
      return;
    }

    container.innerHTML = "";
    professionals.forEach((p) => {
      const col = document.createElement("div");
      col.className = "col-md-6 col-lg-4";
      col.innerHTML = `
        <article class="card professional-card h-100">
          <div class="card-body">
            <h3 class="card-title h5">${p.name}</h3>
            <p class="card-subtitle mb-2 text-muted">${p.specialty}</p>
            <p class="card-text small">${p.approach ?? ""}</p>
            <p class="card-text small mb-1">
              <strong>Cidade:</strong> ${p.city ?? "Atendimento online"}
            </p>
            <p class="card-text small mb-1">
              <strong>Atendimento online:</strong> ${p.onlineSessions ? "Sim" : "Não"}
            </p>
            <p class="card-text small">
              <strong>Contato:</strong> ${p.contactEmail ?? "—"}
            </p>
          </div>
        </article>
      `;
      container.appendChild(col);
    });
  } catch (error) {
    container.innerHTML = `<div class="col-12 text-danger">Não foi possível carregar os profissionais no momento.</div>`;
  }
}

async function loadSupportRequests() {
  const list = document.getElementById("requests-list");
  if (!list) return;

  try {
    const response = await fetch("/api/SupportRequestsApi");
    if (!response.ok) throw new Error("Erro ao carregar pedidos");
    const requests = await response.json();

    if (!Array.isArray(requests) || requests.length === 0) {
      list.innerHTML = '<div class="text-muted">Seja a primeira pessoa a pedir apoio hoje.</div>';
      return;
    }

    list.innerHTML = "";
    requests.slice(0, 5).forEach((r) => {
      const item = document.createElement("div");
      const created = new Date(r.createdAt);
      item.className = "list-group-item";
      item.innerHTML = `
        <div class="d-flex justify-content-between">
          <strong>${r.topic}</strong>
          <span class="text-muted small">${created.toLocaleString()}</span>
        </div>
        <p class="mb-0 small">${r.message}</p>
      `;
      list.appendChild(item);
    });
  } catch (error) {
    list.innerHTML = `<div class="text-danger">Não foi possível carregar os pedidos no momento.</div>`;
  }
}

async function setupSupportForm() {
  const form = document.getElementById("request-support-form");
  const feedback = document.getElementById("support-feedback");
  if (!form || !feedback) return;

  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    feedback.classList.remove("text-success", "text-danger");
    feedback.textContent = "Enviando seu pedido com cuidado...";

    const body = {
      userName: form.userName.value,
      userEmail: form.userEmail.value,
      city: form.city.value || null,
      topic: form.topic.value,
      message: form.message.value,
    };

    try {
      const response = await fetch("/api/SupportRequestsApi", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body),
      });

      if (!response.ok) {
        throw new Error("Erro ao enviar pedido");
      }

      form.reset();
      feedback.classList.add("text-success");
      feedback.textContent = "Pedido enviado com sucesso! Em breve um profissional poderá entrar em contato.";
      await loadSupportRequests();
    } catch (error) {
      feedback.classList.add("text-danger");
      feedback.textContent = "Não foi possível enviar seu pedido. Tente novamente em alguns minutos.";
    }
  });
}

async function setupAiSupport() {
  const form = document.getElementById("ai-support-form");
  const textarea = document.getElementById("aiQuestion");
  const feedback = document.getElementById("ai-support-feedback");
  const responseBox = document.getElementById("ai-response");
  if (!form || !textarea || !feedback || !responseBox) return;

  form.addEventListener("submit", async (e) => {
    e.preventDefault();

    const text = textarea.value.trim();
    if (!text) return;

    feedback.classList.remove("text-success", "text-danger");
    feedback.textContent = "A IA está preparando uma resposta acolhedora...";
    responseBox.textContent = "";

    try {
      const response = await fetch("/api/AiSupportApi", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ question: text }),
      });

      if (!response.ok) {
        throw new Error("Erro ao consultar IA");
      }

      const data = await response.json();
      const answer = data?.answer ?? "Não foi possível obter uma resposta no momento.";
      responseBox.textContent = answer;
      feedback.classList.add("text-success");
      feedback.textContent = "Resposta recebida.";
    } catch (error) {
      responseBox.textContent =
        "Não foi possível falar com a IA agora. Tente novamente em alguns instantes ou procure diretamente um profissional de confiança.";
      feedback.classList.add("text-danger");
      feedback.textContent = "Falha ao consultar a IA.";
    }
  });
}

document.addEventListener("DOMContentLoaded", () => {
  loadProfessionals();
  loadSupportRequests();
  setupSupportForm();
  setupAiSupport();
});


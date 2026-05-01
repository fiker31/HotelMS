/* =========================================
   Hotel Luxe — Main JavaScript
   ========================================= */

document.addEventListener('DOMContentLoaded', function () {

  // ── Sidebar Toggle ──────────────────────────────────────
  const toggleBtn = document.getElementById('sidebar-toggle');
  const sidebar   = document.getElementById('sidebar');
  const main      = document.getElementById('main-content');

  const isMobile = () => window.innerWidth <= 768;

  const savedState = localStorage.getItem('sidebarCollapsed');
  if (savedState === 'true' && !isMobile()) {
    sidebar?.classList.add('collapsed');
    main?.classList.add('expanded');
  }

  if (toggleBtn) {
    toggleBtn.addEventListener('click', function () {
      if (isMobile()) {
        sidebar?.classList.toggle('mobile-open');
      } else {
        const collapsed = sidebar?.classList.toggle('collapsed');
        main?.classList.toggle('expanded');
        localStorage.setItem('sidebarCollapsed', collapsed ? 'true' : 'false');
      }
    });
  }

  // Close sidebar on mobile when clicking outside
  document.addEventListener('click', function (e) {
    if (isMobile() && sidebar?.classList.contains('mobile-open')) {
      if (!sidebar.contains(e.target) && e.target !== toggleBtn) {
        sidebar.classList.remove('mobile-open');
      }
    }
  });

  // ── Stat Count-Up Animation ─────────────────────────────
  const countEls = document.querySelectorAll('[data-count-target]');
  if (countEls.length > 0) {
    const countUp = (el, target, duration) => {
      const isCurrency = el.classList.contains('stat-currency');
      let start = null;
      const step = (ts) => {
        if (!start) start = ts;
        const progress = Math.min((ts - start) / duration, 1);
        const ease = 1 - Math.pow(1 - progress, 3);
        const val = Math.round(ease * target);
        el.textContent = isCurrency ? '$' + val.toLocaleString() : val.toLocaleString();
        if (progress < 1) requestAnimationFrame(step);
        else el.textContent = isCurrency ? '$' + target.toLocaleString() : target.toLocaleString();
      };
      requestAnimationFrame(step);
    };

    const observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          const el = entry.target;
          const target = parseInt(el.dataset.countTarget) || 0;
          countUp(el, target, 1200);
          observer.unobserve(el);
        }
      });
    }, { threshold: 0.2 });

    countEls.forEach(el => observer.observe(el));
  }

  // ── Clickable Table Rows ────────────────────────────────
  document.querySelectorAll('tr[data-href]').forEach(row => {
    row.style.cursor = 'pointer';
    row.addEventListener('click', function (e) {
      if (!e.target.closest('.action-btns') && !e.target.closest('button') && !e.target.closest('a')) {
        window.location.href = row.dataset.href;
      }
    });
  });

  // ── Active Nav Highlighting ─────────────────────────────
  const currentPath = window.location.pathname.toLowerCase();
  document.querySelectorAll('.nav-link').forEach(link => {
    const href = link.getAttribute('href')?.toLowerCase();
    if (href && currentPath.startsWith(href) && href !== '/') {
      link.classList.add('active');
    }
  });

  // ── Delete Confirm Modal ────────────────────────────────
  const modal       = document.getElementById('confirm-modal');
  const cancelBtn   = document.getElementById('modal-cancel');
  const confirmBtn  = document.getElementById('modal-confirm');
  let pendingForm   = null;

  document.querySelectorAll('.delete-form').forEach(form => {
    form.addEventListener('submit', function (e) {
      e.preventDefault();
      pendingForm = form;
      if (modal) {
        modal.style.display = 'flex';
        modal.style.animation = 'none';
        modal.offsetHeight; // reflow
        modal.style.animation = '';
      }
    });
  });

  cancelBtn?.addEventListener('click', () => {
    if (modal) modal.style.display = 'none';
    pendingForm = null;
  });

  confirmBtn?.addEventListener('click', () => {
    if (pendingForm) {
      pendingForm.submit();
    }
    if (modal) modal.style.display = 'none';
  });

  modal?.addEventListener('click', (e) => {
    if (e.target === modal) {
      modal.style.display = 'none';
      pendingForm = null;
    }
  });

  // ── Auto-dismiss Alerts ─────────────────────────────────
  const autoAlert = document.getElementById('auto-alert');
  if (autoAlert) {
    setTimeout(() => {
      autoAlert.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
      autoAlert.style.opacity = '0';
      autoAlert.style.transform = 'translateY(-8px)';
      setTimeout(() => autoAlert.remove(), 500);
    }, 4000);
  }

  // ── Table Row IntersectionObserver Animations ───────────
  const rows = document.querySelectorAll('.data-table tbody tr');
  if (rows.length > 0) {
    const rowObserver = new IntersectionObserver((entries) => {
      entries.forEach((entry, i) => {
        if (entry.isIntersecting) {
          entry.target.style.animationDelay = `${i * 0.03}s`;
          entry.target.style.animation = 'fadeInUp 0.35s ease both';
          rowObserver.unobserve(entry.target);
        }
      });
    }, { threshold: 0.05 });
    rows.forEach(row => rowObserver.observe(row));
  }

  // ── Floating Label: has-value tracking ─────────────────
  document.querySelectorAll('.form-input').forEach(input => {
    const updateLabel = () => {
      input.classList.toggle('has-value', input.value.length > 0);
    };
    updateLabel();
    input.addEventListener('input', updateLabel);
  });

  // ── Search Input — clear on Escape ─────────────────────
  document.querySelectorAll('.search-input').forEach(inp => {
    inp.addEventListener('keydown', e => {
      if (e.key === 'Escape') {
        inp.value = '';
        inp.closest('form')?.submit();
      }
    });
  });

  // ── Keyboard Shortcuts ──────────────────────────────────
  document.addEventListener('keydown', e => {
    if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
      e.preventDefault();
      document.querySelector('.search-input')?.focus();
    }
  });

});

// ── Toast Notification ────────────────────────────────────
function showToast(message, type = 'success') {
  const toast = document.createElement('div');
  toast.className = `toast toast-${type}`;
  toast.innerHTML = `<i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'}"></i> ${message}`;
  toast.style.cssText = `
    position: fixed; bottom: 1.5rem; right: 1.5rem;
    background: ${type === 'success' ? 'rgba(39,174,96,0.9)' : 'rgba(231,76,60,0.9)'};
    color: #fff; padding: 0.875rem 1.25rem; border-radius: 10px;
    font-size: 0.875rem; font-family: Inter, sans-serif;
    display: flex; align-items: center; gap: 0.6rem;
    box-shadow: 0 8px 32px rgba(0,0,0,0.35);
    z-index: 9999;
    animation: slideToastIn 0.35s cubic-bezier(0.34,1.56,0.64,1) both;
  `;
  document.body.appendChild(toast);
  setTimeout(() => {
    toast.style.animation = 'fadeOut 0.3s ease forwards';
    setTimeout(() => toast.remove(), 300);
  }, 3500);
}

// Inject toast keyframes once
if (!document.getElementById('toast-styles')) {
  const style = document.createElement('style');
  style.id = 'toast-styles';
  style.textContent = `
    @keyframes slideToastIn { from { opacity:0; transform: translateX(40px); } to { opacity:1; transform: translateX(0); } }
    @keyframes fadeOut { from { opacity:1; } to { opacity:0; transform: translateY(8px); } }
  `;
  document.head.appendChild(style);
}

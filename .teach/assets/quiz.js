// Componente de quiz das aulas. Markup esperado:
//   <div class="q" data-a="INDICE_0_BASED_DO_BOTAO_CERTO"> <p>…</p> <button>…</button>… <div class="fb"></div> </div>
document.querySelectorAll('.q').forEach(q => {
  const right = +q.dataset.a, fb = q.querySelector('.fb');
  q.querySelectorAll('button').forEach((b, i) => b.onclick = () => {
    q.querySelectorAll('button').forEach(x => x.disabled = true);
    b.classList.add(i === right ? 'right' : 'wrong');
    if (i !== right) q.querySelectorAll('button')[right].classList.add('right');
    fb.textContent = i === right ? '✓ Isso.' : '✗ A verde é a correta — revê a seção acima.';
  });
});

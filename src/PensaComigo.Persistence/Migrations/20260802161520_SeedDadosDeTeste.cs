using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensaComigo.Persistence.Migrations
{
    /// <summary>
    /// Massa de teste para desenvolvimento. NÃO altera schema — só INSERT.
    /// Ids fixos + ON CONFLICT DO NOTHING: reaplicar não duplica.
    /// Usuários ficam de fora (já vêm do HasData da migration inicial).
    /// Enum TipoBloco no jsonb é número (0=Texto, 1=Imagem, 2=Link), igual ao
    /// que o JsonSerializerDefaults.Web grava.
    /// </summary>
    public partial class SeedDadosDeTeste : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
-- ---------------------------------------------------------------- tags (12)
INSERT INTO tags (id, nome, slug, data_criacao) VALUES
  ('c1000000-0000-0000-0000-000000000001', 'Ansiedade',        'ansiedade',        '2026-01-05 00:00:00+00'),
  ('c1000000-0000-0000-0000-000000000002', 'Autoconhecimento', 'autoconhecimento', '2026-01-06 00:00:00+00'),
  ('c1000000-0000-0000-0000-000000000003', 'Relacionamentos',  'relacionamentos',  '2026-01-07 00:00:00+00'),
  ('c1000000-0000-0000-0000-000000000004', 'Saúde Mental',     'saude-mental',     '2026-01-08 00:00:00+00'),
  ('c1000000-0000-0000-0000-000000000005', 'Terapia',          'terapia',          '2026-01-09 00:00:00+00'),
  ('c1000000-0000-0000-0000-000000000006', 'Rotina',           'rotina',           '2026-01-10 00:00:00+00'),
  ('c1000000-0000-0000-0000-000000000007', 'Luto',             'luto',             '2026-01-11 00:00:00+00'),
  ('c1000000-0000-0000-0000-000000000008', 'Maternidade',      'maternidade',      '2026-01-12 00:00:00+00'),
  ('c1000000-0000-0000-0000-000000000009', 'Trabalho',         'trabalho',         '2026-01-13 00:00:00+00'),
  ('c1000000-0000-0000-0000-000000000010', 'Sono',             'sono',             '2026-01-14 00:00:00+00'),
  ('c1000000-0000-0000-0000-000000000011', 'Autoestima',       'autoestima',       '2026-01-15 00:00:00+00'),
  ('c1000000-0000-0000-0000-000000000012', 'Mindfulness',      'mindfulness',      '2026-01-16 00:00:00+00')
ON CONFLICT (id) DO NOTHING;

-- --------------------------------------------------------------- posts (10)
-- autor a1…0001 = Antonio, a1…0002 = Jessica
INSERT INTO posts (id, titulo, slug, conteudo, imagem_capa, tempo_leitura,
                   qtd_curtidas, qtd_visualizacoes, autor_id, data_criacao, data_atualizacao) VALUES
  ('b1000000-0000-0000-0000-000000000001', 'Quando a ansiedade chega sem avisar', 'quando-a-ansiedade-chega-sem-avisar',
   '[{"id":"11111111-0000-0000-0000-000000000001","tipo":0,"ordem":0,"html":"<p>Ela raramente bate na porta. Aparece no meio da reunião, no ônibus, às três da manhã.</p>"},
     {"id":"11111111-0000-0000-0000-000000000002","tipo":0,"ordem":1,"html":"<p>Nomear o que se sente já é metade do caminho para atravessar.</p>"}]'::jsonb,
   'https://picsum.photos/seed/pc1/1200/630', 4, 0, 312, 'a1000000-0000-0000-0000-000000000001', '2026-02-01 00:00:00+00', '2026-02-01 00:00:00+00'),

  ('b1000000-0000-0000-0000-000000000002', 'O que ninguém conta sobre começar terapia', 'o-que-ninguem-conta-sobre-comecar-terapia',
   '[{"id":"11111111-0000-0000-0000-000000000003","tipo":0,"ordem":0,"html":"<p>As primeiras sessões são estranhas. É esperado.</p>"},
     {"id":"11111111-0000-0000-0000-000000000004","tipo":2,"ordem":1,"linkUrl":"https://site.cfp.org.br","linkTitulo":"Conselho Federal de Psicologia","linkDescricao":"Como encontrar um profissional registrado","linkSiteName":"CFP"}]'::jsonb,
   'https://picsum.photos/seed/pc2/1200/630', 6, 0, 508, 'a1000000-0000-0000-0000-000000000002', '2026-02-04 00:00:00+00', '2026-02-05 00:00:00+00'),

  ('b1000000-0000-0000-0000-000000000003', 'Rotina não é prisão', 'rotina-nao-e-prisao',
   '[{"id":"11111111-0000-0000-0000-000000000005","tipo":0,"ordem":0,"html":"<p>Rotina bem desenhada devolve energia para o que importa.</p>"},
     {"id":"11111111-0000-0000-0000-000000000006","tipo":1,"ordem":1,"imagemPath":"posts/rotina.jpg","imagemUrl":"https://picsum.photos/seed/pc3b/800/600","alt":"Caderno aberto com uma lista de tarefas","aspectRatio":1.3333}]'::jsonb,
   'https://picsum.photos/seed/pc3/1200/630', 3, 0, 197, 'a1000000-0000-0000-0000-000000000001', '2026-02-08 00:00:00+00', '2026-02-08 00:00:00+00'),

  ('b1000000-0000-0000-0000-000000000004', 'Dormir mal muda tudo', 'dormir-mal-muda-tudo',
   '[{"id":"11111111-0000-0000-0000-000000000007","tipo":0,"ordem":0,"html":"<p>Antes de tratar o humor, olhe para o sono.</p>"}]'::jsonb,
   'https://picsum.photos/seed/pc4/1200/630', 5, 0, 421, 'a1000000-0000-0000-0000-000000000002', '2026-02-11 00:00:00+00', '2026-02-12 00:00:00+00'),

  ('b1000000-0000-0000-0000-000000000005', 'Limites em relacionamentos: um guia curto', 'limites-em-relacionamentos-um-guia-curto',
   '[{"id":"11111111-0000-0000-0000-000000000008","tipo":0,"ordem":0,"html":"<p>Dizer não é uma frase completa.</p>"},
     {"id":"11111111-0000-0000-0000-000000000009","tipo":0,"ordem":1,"html":"<p>Limite não é muro; é porta com maçaneta do lado de dentro.</p>"}]'::jsonb,
   'https://picsum.photos/seed/pc5/1200/630', 7, 0, 664, 'a1000000-0000-0000-0000-000000000001', '2026-02-15 00:00:00+00', '2026-02-15 00:00:00+00'),

  ('b1000000-0000-0000-0000-000000000006', 'Luto não tem prazo', 'luto-nao-tem-prazo',
   '[{"id":"11111111-0000-0000-0000-000000000010","tipo":0,"ordem":0,"html":"<p>Não existe cronograma para a saudade.</p>"}]'::jsonb,
   'https://picsum.photos/seed/pc6/1200/630', 4, 0, 233, 'a1000000-0000-0000-0000-000000000002', '2026-02-18 00:00:00+00', '2026-02-18 00:00:00+00'),

  ('b1000000-0000-0000-0000-000000000007', 'Maternidade real, sem filtro', 'maternidade-real-sem-filtro',
   '[{"id":"11111111-0000-0000-0000-000000000011","tipo":0,"ordem":0,"html":"<p>Amar muito e estar exausta cabem na mesma frase.</p>"},
     {"id":"11111111-0000-0000-0000-000000000012","tipo":1,"ordem":1,"imagemPath":"posts/maternidade.jpg","imagemUrl":"https://picsum.photos/seed/pc7b/800/800","alt":"Mãos de adulto e de bebê","aspectRatio":1.0}]'::jsonb,
   'https://picsum.photos/seed/pc7/1200/630', 8, 0, 902, 'a1000000-0000-0000-0000-000000000002', '2026-02-22 00:00:00+00', '2026-02-23 00:00:00+00'),

  ('b1000000-0000-0000-0000-000000000008', 'Burnout começa devagar', 'burnout-comeca-devagar',
   '[{"id":"11111111-0000-0000-0000-000000000013","tipo":0,"ordem":0,"html":"<p>Ninguém queima de uma vez. Queima aos poucos, achando que é só cansaço.</p>"}]'::jsonb,
   'https://picsum.photos/seed/pc8/1200/630', 6, 0, 745, 'a1000000-0000-0000-0000-000000000001', '2026-02-25 00:00:00+00', '2026-02-25 00:00:00+00'),

  ('b1000000-0000-0000-0000-000000000009', 'Autoestima não é autoengano', 'autoestima-nao-e-autoengano',
   '[{"id":"11111111-0000-0000-0000-000000000014","tipo":0,"ordem":0,"html":"<p>Gostar de si não exige mentir para si.</p>"}]'::jsonb,
   'https://picsum.photos/seed/pc9/1200/630', 5, 0, 388, 'a1000000-0000-0000-0000-000000000002', '2026-03-01 00:00:00+00', '2026-03-01 00:00:00+00'),

  ('b1000000-0000-0000-0000-000000000010', 'Cinco minutos de atenção plena', 'cinco-minutos-de-atencao-plena',
   '[{"id":"11111111-0000-0000-0000-000000000015","tipo":0,"ordem":0,"html":"<p>Um exercício curto, para fazer sentado, agora.</p>"},
     {"id":"11111111-0000-0000-0000-000000000016","tipo":2,"ordem":1,"linkUrl":"https://www.nhs.uk/mental-health/self-help/tips-and-support/mindfulness/","linkTitulo":"Mindfulness (NHS)","linkDescricao":"Material introdutório do serviço público de saúde britânico","linkSiteName":"NHS"}]'::jsonb,
   'https://picsum.photos/seed/pc10/1200/630', 3, 0, 176, 'a1000000-0000-0000-0000-000000000001', '2026-03-05 00:00:00+00', '2026-03-06 00:00:00+00')
ON CONFLICT (id) DO NOTHING;

-- ------------------------------------------------- post_tags (N:N, 20 pares)
INSERT INTO post_tags (post_id, tag_id) VALUES
  ('b1000000-0000-0000-0000-000000000001', 'c1000000-0000-0000-0000-000000000001'),
  ('b1000000-0000-0000-0000-000000000001', 'c1000000-0000-0000-0000-000000000004'),
  ('b1000000-0000-0000-0000-000000000002', 'c1000000-0000-0000-0000-000000000005'),
  ('b1000000-0000-0000-0000-000000000002', 'c1000000-0000-0000-0000-000000000004'),
  ('b1000000-0000-0000-0000-000000000003', 'c1000000-0000-0000-0000-000000000006'),
  ('b1000000-0000-0000-0000-000000000003', 'c1000000-0000-0000-0000-000000000002'),
  ('b1000000-0000-0000-0000-000000000004', 'c1000000-0000-0000-0000-000000000010'),
  ('b1000000-0000-0000-0000-000000000004', 'c1000000-0000-0000-0000-000000000004'),
  ('b1000000-0000-0000-0000-000000000005', 'c1000000-0000-0000-0000-000000000003'),
  ('b1000000-0000-0000-0000-000000000005', 'c1000000-0000-0000-0000-000000000002'),
  ('b1000000-0000-0000-0000-000000000006', 'c1000000-0000-0000-0000-000000000007'),
  ('b1000000-0000-0000-0000-000000000006', 'c1000000-0000-0000-0000-000000000004'),
  ('b1000000-0000-0000-0000-000000000007', 'c1000000-0000-0000-0000-000000000008'),
  ('b1000000-0000-0000-0000-000000000007', 'c1000000-0000-0000-0000-000000000003'),
  ('b1000000-0000-0000-0000-000000000008', 'c1000000-0000-0000-0000-000000000009'),
  ('b1000000-0000-0000-0000-000000000008', 'c1000000-0000-0000-0000-000000000004'),
  ('b1000000-0000-0000-0000-000000000009', 'c1000000-0000-0000-0000-000000000011'),
  ('b1000000-0000-0000-0000-000000000009', 'c1000000-0000-0000-0000-000000000002'),
  ('b1000000-0000-0000-0000-000000000010', 'c1000000-0000-0000-0000-000000000012'),
  ('b1000000-0000-0000-0000-000000000010', 'c1000000-0000-0000-0000-000000000006')
ON CONFLICT DO NOTHING;

-- -------------------------------------------------------- comentarios (14)
-- d1…0011 e d1…0012 são RESPOSTAS (parent_id preenchido);
-- d1…0013 e d1…0014 estão pendentes de moderação (aprovado = false).
INSERT INTO comentarios (id, post_id, parent_id, autor, conteudo, aprovado, data_criacao) VALUES
  ('d1000000-0000-0000-0000-000000000001', 'b1000000-0000-0000-0000-000000000001', NULL, 'Marina',  'Li isso às 3 da manhã. Chegou na hora certa.', true,  '2026-02-02 00:00:00+00'),
  ('d1000000-0000-0000-0000-000000000002', 'b1000000-0000-0000-0000-000000000001', NULL, 'Rafael',  'A parte de nomear o que se sente me pegou.',   true,  '2026-02-03 00:00:00+00'),
  ('d1000000-0000-0000-0000-000000000003', 'b1000000-0000-0000-0000-000000000002', NULL, 'Camila',  'Comecei semana passada. É estranho mesmo.',    true,  '2026-02-06 00:00:00+00'),
  ('d1000000-0000-0000-0000-000000000004', 'b1000000-0000-0000-0000-000000000003', NULL, 'Diego',   'Vou testar a rotina de manhã.',                true,  '2026-02-09 00:00:00+00'),
  ('d1000000-0000-0000-0000-000000000005', 'b1000000-0000-0000-0000-000000000004', NULL, 'Helena',  'Sono foi a primeira coisa que ajustei.',       true,  '2026-02-13 00:00:00+00'),
  ('d1000000-0000-0000-0000-000000000006', 'b1000000-0000-0000-0000-000000000005', NULL, 'Bruno',   'Guardei a frase da maçaneta.',                 true,  '2026-02-16 00:00:00+00'),
  ('d1000000-0000-0000-0000-000000000007', 'b1000000-0000-0000-0000-000000000006', NULL, 'Sofia',   'Perdi minha avó em janeiro. Obrigada.',        true,  '2026-02-19 00:00:00+00'),
  ('d1000000-0000-0000-0000-000000000008', 'b1000000-0000-0000-0000-000000000007', NULL, 'Larissa', 'Chorei lendo. É exatamente isso.',             true,  '2026-02-24 00:00:00+00'),
  ('d1000000-0000-0000-0000-000000000009', 'b1000000-0000-0000-0000-000000000008', NULL, 'Thiago',  'Achei que era só cansaço por dois anos.',      true,  '2026-02-26 00:00:00+00'),
  ('d1000000-0000-0000-0000-000000000010', 'b1000000-0000-0000-0000-000000000010', NULL, 'Paula',   'Fiz os cinco minutos agora. Funcionou.',       true,  '2026-03-07 00:00:00+00'),
  ('d1000000-0000-0000-0000-000000000011', 'b1000000-0000-0000-0000-000000000001', 'd1000000-0000-0000-0000-000000000001', 'Antonio Ramon', 'Que bom que ajudou, Marina.', true, '2026-02-02 00:00:00+00'),
  ('d1000000-0000-0000-0000-000000000012', 'b1000000-0000-0000-0000-000000000007', 'd1000000-0000-0000-0000-000000000008', 'Jessica Rose',  'Obrigada por dividir isso.',  true, '2026-02-24 00:00:00+00'),
  ('d1000000-0000-0000-0000-000000000013', 'b1000000-0000-0000-0000-000000000005', NULL, 'Anônimo', 'Comentário aguardando moderação.',             false, '2026-02-17 00:00:00+00'),
  ('d1000000-0000-0000-0000-000000000014', 'b1000000-0000-0000-0000-000000000009', NULL, 'Visitante', 'Outro pendente de aprovação.',               false, '2026-03-02 00:00:00+00')
ON CONFLICT (id) DO NOTHING;

-- ------------------------------------------------------------- likes (16)
-- viewer_hash é único por post (índice composto) — mesmo visitante curte posts diferentes.
INSERT INTO likes (id, post_id, viewer_hash, data_criacao) VALUES
  ('e1000000-0000-0000-0000-000000000001', 'b1000000-0000-0000-0000-000000000001', 'hash-visitante-01', '2026-02-02 00:00:00+00'),
  ('e1000000-0000-0000-0000-000000000002', 'b1000000-0000-0000-0000-000000000001', 'hash-visitante-02', '2026-02-02 00:00:00+00'),
  ('e1000000-0000-0000-0000-000000000003', 'b1000000-0000-0000-0000-000000000001', 'hash-visitante-03', '2026-02-03 00:00:00+00'),
  ('e1000000-0000-0000-0000-000000000004', 'b1000000-0000-0000-0000-000000000002', 'hash-visitante-01', '2026-02-06 00:00:00+00'),
  ('e1000000-0000-0000-0000-000000000005', 'b1000000-0000-0000-0000-000000000002', 'hash-visitante-04', '2026-02-06 00:00:00+00'),
  ('e1000000-0000-0000-0000-000000000006', 'b1000000-0000-0000-0000-000000000003', 'hash-visitante-05', '2026-02-09 00:00:00+00'),
  ('e1000000-0000-0000-0000-000000000007', 'b1000000-0000-0000-0000-000000000004', 'hash-visitante-02', '2026-02-13 00:00:00+00'),
  ('e1000000-0000-0000-0000-000000000008', 'b1000000-0000-0000-0000-000000000005', 'hash-visitante-03', '2026-02-16 00:00:00+00'),
  ('e1000000-0000-0000-0000-000000000009', 'b1000000-0000-0000-0000-000000000005', 'hash-visitante-06', '2026-02-16 00:00:00+00'),
  ('e1000000-0000-0000-0000-000000000010', 'b1000000-0000-0000-0000-000000000006', 'hash-visitante-07', '2026-02-19 00:00:00+00'),
  ('e1000000-0000-0000-0000-000000000011', 'b1000000-0000-0000-0000-000000000007', 'hash-visitante-01', '2026-02-24 00:00:00+00'),
  ('e1000000-0000-0000-0000-000000000012', 'b1000000-0000-0000-0000-000000000007', 'hash-visitante-08', '2026-02-24 00:00:00+00'),
  ('e1000000-0000-0000-0000-000000000013', 'b1000000-0000-0000-0000-000000000008', 'hash-visitante-09', '2026-02-26 00:00:00+00'),
  ('e1000000-0000-0000-0000-000000000014', 'b1000000-0000-0000-0000-000000000009', 'hash-visitante-10', '2026-03-02 00:00:00+00'),
  ('e1000000-0000-0000-0000-000000000015', 'b1000000-0000-0000-0000-000000000010', 'hash-visitante-05', '2026-03-07 00:00:00+00'),
  ('e1000000-0000-0000-0000-000000000016', 'b1000000-0000-0000-0000-000000000010', 'hash-visitante-11', '2026-03-07 00:00:00+00')
ON CONFLICT (id) DO NOTHING;

-- Contador desnormalizado do post fica coerente com os likes inseridos.
UPDATE posts p
   SET qtd_curtidas = (SELECT count(*) FROM likes l WHERE l.post_id = p.id);
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Apaga só o que esta migration inseriu (prefixos de id reservados).
            migrationBuilder.Sql("""
DELETE FROM likes       WHERE id::text LIKE 'e1000000-%';
DELETE FROM comentarios WHERE id::text LIKE 'd1000000-%';
DELETE FROM post_tags   WHERE post_id::text LIKE 'b1000000-%';
DELETE FROM posts       WHERE id::text LIKE 'b1000000-%';
DELETE FROM tags        WHERE id::text LIKE 'c1000000-%';
""");
        }
    }
}

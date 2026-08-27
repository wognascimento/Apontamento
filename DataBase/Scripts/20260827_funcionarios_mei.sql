CREATE TABLE IF NOT EXISTS ht.funcionarios_mei
(
    setor character varying(50) NOT NULL,
    nome_apelido character varying(100) NOT NULL,
    admissao timestamp with time zone NOT NULL,
    demissao timestamp with time zone,
    local_galpao character varying(10) NOT NULL DEFAULT 'MEI',
    codfun integer NOT NULL DEFAULT nextval('ht.funcionarios_codfun_seq'::regclass),
    CONSTRAINT funcionarios_mei_pkey PRIMARY KEY (codfun)
);

CREATE INDEX IF NOT EXISTS idx_funcionarios_mei_setor
    ON ht.funcionarios_mei (setor);

CREATE INDEX IF NOT EXISTS idx_funcionarios_mei_nome_apelido
    ON ht.funcionarios_mei (nome_apelido);

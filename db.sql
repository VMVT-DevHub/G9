EXEC UserSetup;
EXEC JarSetup;

GRANT USAGE ON SCHEMA public TO g9_app; GRANT USAGE ON SCHEMA jar TO g9_app;	
	
--Public Tables:	
	CREATE TABLE public.daznumas (dzn_id integer NOT NULL GENERATED ALWAYS AS IDENTITY, dzn_grupe varchar(3) NOT NULL, dzn_pavad varchar(255) NOT NULL, dzn_nuo integer NOT NULL, dzn_iki integer NOT NULL, dzn_kartai integer NOT NULL, dzn_laikas integer NOT NULL, CONSTRAINT g9_daznumas_pkey PRIMARY KEY (dzn_id));
	CREATE TABLE public.deklaravimas (dkl_id integer NOT NULL GENERATED ALWAYS AS IDENTITY, dkl_gvts bigint NOT NULL, dkl_metai integer NOT NULL, dkl_stebesena integer NOT NULL, dkl_status integer NOT NULL, dkl_kiekis double precision, dkl_vartot integer, dkl_deklar_date timestamp(3), dkl_deklar_user varchar(255), dkl_deklar_user_id uuid, dkl_modif_date timestamp(3), dkl_modif_user varchar(255), dkl_modif_user_id uuid, CONSTRAINT g9_deklaravimas_pkey PRIMARY KEY (dkl_id));
	CREATE TABLE public.gvts (vkl_id bigint NOT NULL, vkl_ja bigint, vkl_title varchar(255), vkl_saviv varchar(255), vkl_adresas varchar(255), CONSTRAINT g9_gvts_pkey PRIMARY KEY (vkl_id));
	CREATE TABLE public.lookup (lkp_id integer NOT NULL GENERATED ALWAYS AS IDENTITY, lkp_group varchar(30) NOT NULL, lkp_key varchar(30), lkp_num integer, lkp_value varchar(255), CONSTRAINT g9_public_lookup_pkey PRIMARY KEY (lkp_id));
	CREATE TABLE public.rodikliai (rod_id integer NOT NULL, rod_grupe integer NOT NULL, rod_kodas varchar(30) NOT NULL, rod_rodiklis varchar(255) NOT NULL, rod_daznumas varchar(3), rod_min double precision NOT NULL, rod_max double precision NOT NULL, rod_step double precision NOT NULL, rod_vnt varchar(30) NOT NULL, rod_aprasymas text, CONSTRAINT g9_rodikliai_pkey PRIMARY KEY (rod_id));
	CREATE TABLE public.stebesenos (stb_id integer NOT NULL GENERATED ALWAYS AS IDENTITY, stb_stebesenos integer, stb_rodiklis integer, PRIMARY KEY (stb_id));
	CREATE TABLE public.reiksmes (rks_id bigint NOT NULL GENERATED ALWAYS AS IDENTITY, rks_deklar integer, rks_rodiklis integer, rks_date date, rks_reiksme double precision, rks_suvedimas integer, rks_user uuid, rks_date_add timestamp(0) DEFAULT timezone('utc', now()), CONSTRAINT reiksmes_pkey PRIMARY KEY (rks_id));
	CREATE TABLE public.valid_trukumas (vld_id integer NOT NULL GENERATED ALWAYS AS IDENTITY, vld_deklar int NOT NULL, vld_rodiklis integer NOT NULL, vld_suvesta integer NOT NULL, vld_reikia integer NOT NULL, vld_tvirtinti boolean, vld_pastabos text, vld_user uuid, vld_date_add timestamp(0) DEFAULT timezone('utc'::text, now()), vld_date_modif timestamp(0), vld_del boolean NOT NULL DEFAULT false, CONSTRAINT valid_trukumas_pkey PRIMARY KEY (vld_id));
	CREATE TABLE public.valid_kartojasi (vld_id integer NOT NULL GENERATED ALWAYS AS IDENTITY, vld_deklar int NOT NULL, vld_rodiklis integer NOT NULL, vld_data date NOT NULL, vld_reiksme double precision NOT NULL, vld_tvirtinti boolean, vld_pastabos text, vld_user uuid, vld_date_add timestamp(0) DEFAULT timezone('utc'::text, now()), vld_date_modif timestamp(0), vld_del boolean NOT NULL DEFAULT false, CONSTRAINT valid_kartojasi_pkey PRIMARY KEY (vld_id))
	CREATE TABLE public.valid_virsija (vld_id integer NOT NULL GENERATED ALWAYS AS IDENTITY, vld_deklar int NOT NULL, vld_rodiklis integer NOT NULL, vld_nuo date NOT NULL, vld_iki date NOT NULL, vld_max double precision NOT NULL, vld_nereiksm boolean, vld_nereiksm_apras text, vld_zmones int, vld_tipas varchar(255), vld_loq_reiksme varchar(255), vld_loq_verte varchar(255), vld_statusas varchar(255), vld_tvirtinti boolean, vld_pastabos text, vld_user uuid, vld_date_add timestamp(0) DEFAULT timezone('utc'::text, now()), vld_date_modif timestamp(0), vld_del boolean NOT NULL DEFAULT false, CONSTRAINT valid_virsija_pkey PRIMARY KEY (vld_id));
	GRANT SELECT ON TABLE public.daznumas TO g9_app; GRANT ALL ON TABLE public.deklaravimas TO g9_app; GRANT SELECT ON TABLE public.gvts TO g9_app; GRANT SELECT ON TABLE public.lookup TO g9_app; GRANT SELECT ON TABLE public.rodikliai TO g9_app; GRANT SELECT ON TABLE public.stebesenos TO g9_app; GRANT SELECT,INSERT,DELETE ON TABLE public.reiksmes TO g9_app;

--Public Views:	
	CREATE VIEW public.v_deklar AS SELECT dkl_gvts AS "GVTS", dkl_metai AS "Metai", dkl_stebesena As "Stebesenos", dkl_status AS "Statusas", dkl_kiekis AS "Kiekis", dkl_vartot AS "Vartotojai", dkl_deklar_date AS "DeklarDate", dkl_deklar_user AS "DeklarUser", dkl_modif_date AS "RedagDate", dkl_modif_user AS "RedagUser" FROM deklaravimas;
	CREATE VIEW public.v_gvts AS SELECT vkl_id AS "ID", vkl_ja AS "JA", vkl_title AS "Title", vkl_adresas AS "Addr" FROM gvts;
	CREATE VIEW public.v_gvts_ja AS SELECT g.vkl_id AS gvts, j.ja_id AS "ID", j.ja_title AS "Title", j.ja_adresas AS "Addr" FROM gvts g LEFT JOIN jar.data j ON g.vkl_ja = j.ja_id WHERE j.ja_id IS NOT NULL;
	CREATE VIEW public.v_rodikliai AS SELECT rod_id as "ID", rod_grupe as "Grupe", rod_kodas as "Kodas", rod_rodiklis as "Pavadinimas", rod_daznumas as "Daznumas", rod_min as "Min", rod_max as "Max", rod_step as "Step", rod_vnt as "Vnt", rod_aprasymas as "Aprasymas" FROM public.rodikliai;
	CREATE VIEW public.v_reiksmes AS SELECT rks_id as "ID", rks_deklar as "Deklaracija", rks_rodiklis as "Rodiklis", rks_date as "Data", rks_reiksme as "Reiksme" FROM public.reiksmes ORDER BY rks_rodiklis,rks_date;
	CREATE VIEW public.v_daznumas AS SELECT dzn_grupe as "Daznumas", dzn_nuo as "Nuo", dzn_iki as "Iki", dzn_kartai as "Kartai", dzn_laikas as "Laikas", l.val as "Daugiklis" FROM public.daznumas as d left join public.lkp_daznumo_daugiklis as l on (d.dzn_laikas=l.key) ORDER BY dzn_id ASC;
	CREATE VIEW public.lkp_rodikliu_grupes AS SELECT lkp_num AS key, lkp_value AS val FROM public.lookup WHERE lkp_group::text = 'RodikliuGrupe'::text;
	CREATE VIEW public.lkp_rodikliu_validacija AS SELECT lkp_num AS key, lkp_value AS val FROM lookup WHERE lkp_group = 'RodikliuGrupe' and lkp_key='Taip';
	CREATE VIEW public.lkp_stebesenos AS SELECT lkp_num as key, lkp_value as val FROM public.lookup WHERE lkp_group='Stebesenos';
	CREATE VIEW public.lkp_statusas AS SELECT lkp_num as key, lkp_value as val FROM public.lookup WHERE lkp_group='Statusas';
	CREATE VIEW public.lkp_daznumas AS SELECT lkp_key as key, lkp_value as val FROM public.lookup WHERE lkp_group='Daznumas';
	CREATE VIEW public.lkp_daznumo_laikas AS SELECT lkp_num as key, lkp_value as val FROM public.lookup WHERE lkp_group='DaznumoLaikas';
	CREATE VIEW public.lkp_daznumo_daugiklis AS SELECT lkp_num as key, try_cast(lkp_key,0::integer) as val FROM public.lookup WHERE lkp_group='DaznumoLaikas';
	GRANT SELECT ON TABLE public.v_gvts TO g9_app; GRANT SELECT ON TABLE public.v_deklar TO g9_app; GRANT SELECT ON TABLE public.v_gvts_ja TO g9_app; GRANT SELECT ON TABLE public.v_rodikliu_grupes TO g9_app; GRANT SELECT ON TABLE public.v_stebesenos TO g9_app;GRANT SELECT ON TABLE public.lkp_statusas TO g9_app; GRANT SELECT ON TABLE public.v_rodikliai TO g9_app;GRANT SELECT ON TABLE public.lkp_daznumas TO g9_app; GRANT SELECT ON TABLE public.lkp_daznumo_laikas TO g9_app; GRANT SELECT ON TABLE public.lkp_daznumo_daugiklis TO g9_app; GRANT SELECT ON TABLE public.v_daznumas TO g9_app; GRANT SELECT ON TABLE public.v_reiksmes TO g9_app;
	
--App Tables:	
	CREATE TABLE app.config (cfg_id integer NOT NULL GENERATED ALWAYS AS IDENTITY, cfg_group varchar(255) NOT NULL, cfg_key varchar(255) NOT NULL, cfg_val varchar(255), cfg_int bigint, cfg_num double precision, cfg_date timestamp(3), cfg_descr text, cfg_text text, CONSTRAINT g9_config_pkey PRIMARY KEY (cfg_id));
	CREATE TABLE app.log_error (log_id bigint NOT NULL GENERATED ALWAYS AS IDENTITY, log_date timestamp(3) DEFAULT timezone('utc', now()), log_code bigint, log_msg varchar(255), log_data text, log_ip varchar(255), CONSTRAINT g9_log_error_pkey PRIMARY KEY (log_id));
	CREATE TABLE app.log_login (log_id bigint NOT NULL GENERATED ALWAYS AS IDENTITY, log_data text, log_date timestamp(3) DEFAULT timezone('utc', now()), CONSTRAINT g9_login_log_pkey PRIMARY KEY (log_id));
	CREATE TABLE app.session (sess_id bigint NOT NULL GENERATED ALWAYS AS IDENTITY, sess_key varchar(255), sess_data text, sess_expire timestamp(0), sess_extended integer, CONSTRAINT g9_session_pkey PRIMARY KEY (sess_id), CONSTRAINT g9_sess_key_unique UNIQUE (sess_key));
	GRANT SELECT, UPDATE ON TABLE app.config TO g9_app; GRANT INSERT ON TABLE app.log_error TO g9_app; GRANT INSERT ON TABLE app.log_login TO g9_app; GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE app.session TO g9_app;
	
--Funkcijos:	
	CREATE FUNCTION public.try_cast(_in text, INOUT _out ANYELEMENT) LANGUAGE plpgsql AS $$ BEGIN EXECUTE format('SELECT %L::%s', $1, pg_typeof(_out)) INTO _out; EXCEPTION WHEN others THEN END $$;
	CREATE FUNCTION public.gvts_users(gvts bigint[])
	RETURNS table("GVTS" bigint, "ID" uuid, "FName" varchar(255), "LName" varchar(255), "Admin" bool, "LastLogin" timestamp(0)) AS $$ BEGIN
	RETURN QUERY WITH rls as (SELECT CONCAT('g9.',b)::varchar(30) as role, b as gvts, false as admin from unnest(gvts) AS b UNION ALL SELECT CONCAT('g9.',b,'.admin')::varchar(30) as role, b as gvts, true as admin from unnest(gvts) AS b)
	   SELECT rls.gvts, usrl_user, user_fname, user_lname, rls.admin, user_dt_login
	   FROM rls LEFT JOIN app.roles on role_name=rls.role LEFT JOIN app.user_roles on role_id=usrl_role LEFT JOIN app.users on user_id=usrl_user
	   WHERE user_id is not null;
	END $$ LANGUAGE plpgsql SECURITY DEFINER;
	GRANT EXECUTE ON FUNCTION public.gvts_users(bigint[]) TO g9_app;
	
	CREATE FUNCTION public.valid_kartojasi(deklar integer) RETURNS TABLE(rodiklis integer, data date, reiksme double precision) LANGUAGE 'plpgsql' AS $$ BEGIN
	   RETURN QUERY SELECT rks_rodiklis, rks_date, rks_reiksme FROM public.reiksmes WHERE rks_deklar=deklar GROUP BY rks_rodiklis, rks_date, rks_reiksme HAVING count(*)>1; END $$;
	CREATE FUNCTION public.valid_kartojasi_get(deklar integer) RETURNS TABLE("ID" integer, "Rodiklis" integer, "Data" date, "Reiksme" double precision, "Patvirtinta" boolean, "Pastabos" text) LANGUAGE 'plpgsql' AS $$
	BEGIN RETURN QUERY SELECT vld_id, vld_rodiklis, vld_data, vld_reiksme, vld_tvirtinti, vld_pastabos FROM public.valid_kartojasi WHERE vld_deklar=deklar and vld_del=false; END; $$;
	CREATE FUNCTION public.valid_kartojasi_set(deklar integer, userid uuid) RETURNS TABLE("ID" integer, "Rodiklis" integer, "Data" date, "Reiksme" double precision, "Patvirtinta" boolean, "Pastabos" text) LANGUAGE 'plpgsql' AS $$ BEGIN
	   WITH curr AS (SELECT rodiklis, data, reiksme FROM public.valid_kartojasi(deklar)),
	   --Pažymėti neegzistuojančius
	   upd as (UPDATE public.valid_kartojasi SET vld_del=true,vld_user=userid,vld_date_modif=timezone('utc'::text, now()) WHERE vld_deklar=deklar AND vld_del=false AND (vld_rodiklis,vld_data,vld_reiksme) NOT IN (SELECT rodiklis, data, reiksme FROM curr) RETURNING *),
	   --Grąžinti ištrintus
	   upf as (UPDATE public.valid_kartojasi SET vld_del=false,vld_user=userid,vld_date_modif=timezone('utc'::text, now()) WHERE vld_deklar=deklar AND vld_del=true AND (vld_rodiklis,vld_data,vld_reiksme) IN (SELECT rodiklis, data, reiksme FROM curr) RETURNING *)
	   --Pridėti naujus
	   INSERT INTO public.valid_kartojasi (vld_rodiklis,vld_data,vld_reiksme,vld_deklar,vld_user) SELECT rodiklis, data, reiksme, deklar, userid FROM curr WHERE (rodiklis, data, reiksme) NOT IN (SELECT vld_rodiklis,vld_data,vld_reiksme FROM public.valid_kartojasi WHERE vld_deklar=deklar);
	   RETURN QUERY SELECT * FROM public.valid_kartojasi_get(deklar);
	END $$; GRANT EXECUTE ON FUNCTION public.valid_kartojasi(integer) TO g9_app; GRANT EXECUTE ON FUNCTION public.valid_kartojasi_get(integer) TO g9_app; GRANT EXECUTE ON FUNCTION public.valid_kartojasi_set(integer,uuid) TO g9_app;
	
	CREATE FUNCTION public.valid_trukumas(deklar integer) 
	RETURNS TABLE(rodiklis integer, suvesta integer, reikia integer) AS $$ DECLARE stb integer; kiekis integer; BEGIN
	SELECT COALESCE(dkl_kiekis,1),dkl_stebesena into stb, kiekis FROM deklaravimas WHERE dkl_id=deklar;
	RETURN QUERY WITH steb as (SELECT stb_rodiklis FROM stebesenos WHERE stb_stebesenos=stb),
	   dzn as (SELECT dzn_grupe,(max(dzn_kartai)*COALESCE(max(l.val),1)) as dzn_kartai FROM daznumas d LEFT JOIN lkp_daznumo_daugiklis l ON d.dzn_laikas = l.key WHERE dzn_nuo <=kiekis AND dzn_iki>=kiekis and dzn_kartai>0 group by dzn_grupe),
	   rod as (SELECT rod_id,dzn_kartai FROM rodikliai INNER JOIN dzn ON (rod_daznumas=dzn_grupe)),
	   rks as (SELECT rks_rodiklis, count(*) as rks_count FROM reiksmes WHERE rks_deklar=deklar group by rks_rodiklis),
	   grp as (SELECT rod_id,dzn_kartai FROM steb INNER JOIN rod on (rod_id=stb_rodiklis))
	SELECT rod_id,COALESCE(rks_count,0)::integer, dzn_kartai FROM grp LEFT JOIN rks ON (rks_rodiklis=rod_id) WHERE dzn_kartai>rks_count or rks_count is null;
	END $$ LANGUAGE 'plpgsql';
	CREATE FUNCTION public.valid_trukumas_get(deklar integer) RETURNS TABLE("ID" integer, "Rodiklis" integer, "Suvesta" integer, "Reikia" integer, "Patvirtinta" boolean, "Pastabos" text) LANGUAGE 'plpgsql' AS $$BEGIN RETURN QUERY
	SELECT vld_id, vld_rodiklis, vld_suvesta, vld_reikia, vld_tvirtinti, vld_pastabos FROM public.valid_trukumas WHERE vld_deklar=deklar and vld_del=false; END; $$;
	CREATE FUNCTION public.valid_trukumas_set(deklar integer, userid uuid) RETURNS TABLE("ID" integer, "Rodiklis" integer, "Suvesta" integer, "Reikia" integer, "Patvirtinta" boolean, "Pastabos" text) LANGUAGE 'plpgsql' AS $$ BEGIN
	   WITH curr AS (SELECT rodiklis, suvesta, reikia FROM public.valid_trukumas(deklar)),
	   --Pažymėti neegzistuojančius
	   upd as (UPDATE public.valid_trukumas SET vld_del=true,vld_user=userid,vld_date_modif=timezone('utc'::text, now()) WHERE vld_deklar=deklar AND vld_del=false AND vld_rodiklis NOT IN (SELECT rodiklis FROM curr) RETURNING *),
	   --Grąžinti ištrintus
	   upf as (UPDATE public.valid_trukumas SET vld_del=false,vld_suvesta=curr.suvesta,vld_reikia=curr.reikia,vld_user=userid,vld_date_modif=timezone('utc'::text, now()) FROM curr WHERE vld_deklar=deklar AND vld_rodiklis=curr.rodiklis AND (vld_del=true or vld_rodiklis<>curr.rodiklis or vld_suvesta<>curr.suvesta) RETURNING *)
	   --Pridėti naujus
	   INSERT INTO public.valid_trukumas (vld_rodiklis,vld_suvesta, vld_reikia,vld_deklar,vld_user) SELECT rodiklis, suvesta, reikia, deklar, userid FROM curr WHERE rodiklis NOT IN (SELECT vld_rodiklis FROM public.valid_trukumas WHERE vld_deklar=deklar);   
	   RETURN QUERY SELECT * FROM public.valid_trukumas_get(deklar);
	END $$; GRANT EXECUTE ON FUNCTION public.valid_trukumas(integer) TO g9_app; GRANT EXECUTE ON FUNCTION public.valid_trukumas_get(integer) TO g9_app; GRANT EXECUTE ON FUNCTION public.valid_trukumas_set(integer,uuid) TO g9_app;

	CREATE FUNCTION public.valid_virsija(in deklar integer, out rodiklis integer, out nuo date, out iki date, out max double precision) RETURNS SETOF RECORD AS $$ DECLARE rec RECORD; BEGIN max:=0;
	   FOR rec IN (WITH 
	      riba as (SELECT rks_rodiklis rod, rks_date dte FROM reiksmes LEFT JOIN rodikliai on (rks_rodiklis=rod_id) WHERE rks_deklar=deklar and (rks_reiksme>rod_max or rks_reiksme<rod_min) GROUP BY rks_rodiklis,rks_date),
	      rod as (SELECT rks_rodiklis, rks_date, rks_reiksme, rod is not null vl_virs FROM reiksmes LEFT JOIN riba on (rks_rodiklis=rod and rks_date=dte) WHERE rks_deklar=deklar)
	      SELECT rks_rodiklis as rod, rks_date as dte, rks_reiksme as val, vl_virs as virs, LEAD(vl_virs) OVER (PARTITION BY rks_rodiklis ORDER BY rks_date) IS DISTINCT FROM vl_virs as ends FROM rod
	   ) LOOP IF rec.virs THEN rodiklis:=rec.rod; iki:=rec.dte; IF nuo IS NULL THEN nuo:=rec.dte; END IF; IF max<rec.val THEN max:=rec.val; END IF; IF rec.ends THEN RETURN NEXT; nuo:=null; END IF; END IF;
	END LOOP; RETURN; END $$ LANGUAGE plpgsql;
	CREATE FUNCTION public.valid_virsija_get(deklar integer) RETURNS TABLE("ID" integer, "Rodiklis" integer, "Nuo" date, "Iki" date, "Max" double precision, "Detales" boolean, "Nereiksmingas" boolean, "NereiksmApras" text, "Zmones" integer, "Tipas" varchar, "LOQReiksme" varchar, "LOQVerte" varchar, "Statusas" varchar, "Patvirtinta" boolean, "Pastabos" text) LANGUAGE 'plpgsql' AS $$BEGIN RETURN QUERY
	WITH rod as (SELECT rod_id as id FROM public.lkp_rodikliu_validacija as vld LEFT JOIN public.rodikliai on (rod_grupe=vld.key)) SELECT vld_id, vld_rodiklis, vld_nuo, vld_iki, vld_max, rod.id is not null, vld_nereiksm, vld_nereiksm_apras, vld_zmones, vld_tipas, vld_loq_reiksme, vld_loq_verte,vld_statusas, vld_tvirtinti, vld_pastabos FROM public.valid_virsija LEFT JOIN rod ON (vld_rodiklis=rod.id) WHERE vld_deklar=deklar and vld_del=false; END; $$;
	CREATE FUNCTION public.valid_virsija_set(deklar integer, userid uuid) RETURNS TABLE("ID" integer, "Rodiklis" integer, "Nuo" date, "Iki" date, "Max" double precision, "Detales" boolean, "Nereiksmingas" boolean, "NereiksmApras" text, "Zmones" integer, "Tipas" varchar, "LOQReiksme" varchar, "LOQVerte" varchar, "Statusas" varchar, "Patvirtinta" boolean, "Pastabos" text) LANGUAGE 'plpgsql' AS $$ BEGIN
	   WITH curr AS (SELECT rodiklis, nuo, iki, max FROM public.valid_virsija(deklar)),
	   --Pažymėti neegzistuojančius
	   upd as (UPDATE public.valid_virsija SET vld_del=true,vld_user=userid,vld_date_modif=timezone('utc'::text, now()) WHERE vld_deklar=deklar AND vld_del=false AND (vld_rodiklis,vld_nuo,vld_iki) NOT IN (SELECT rodiklis,nuo,iki FROM curr) RETURNING *),
	   --Grąžinti ištrintus
	   upf as (UPDATE public.valid_virsija SET vld_del=false,vld_max=curr.max, vld_user=userid,vld_date_modif=timezone('utc'::text, now()) FROM curr WHERE vld_deklar=deklar AND vld_rodiklis=curr.rodiklis AND vld_nuo=curr.nuo AND vld_iki=curr.iki AND (vld_del=true or vld_max<>curr.max) RETURNING *)
	   --Pridėti naujus
	   INSERT INTO public.valid_virsija (vld_rodiklis,vld_nuo,vld_iki,vld_max,vld_deklar,vld_user)  SELECT rodiklis, nuo, iki, max, deklar, userid FROM curr WHERE (rodiklis,nuo,iki) NOT IN (SELECT vld_rodiklis,vld_nuo,vld_iki FROM public.valid_virsija WHERE vld_deklar=deklar);   
	   RETURN QUERY SELECT * FROM public.valid_virsija_get(deklar);
	END $$; GRANT EXECUTE ON FUNCTION public.valid_virsija(integer) TO g9_app; GRANT EXECUTE ON FUNCTION public.valid_virsija_get(integer) TO g9_app; GRANT EXECUTE ON FUNCTION public.valid_virsija_set(integer,uuid) TO g9_app;

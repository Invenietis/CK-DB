-- Version = *

CREATE PROCEDURE CK.sZoneMove
(
	@id int,         -- Ident de l'élément déplacé 
    @id_parent int,  -- Ancêtre de destination 
    @mode char(2),   -- FG : Grand Frère, PF : Petit Frère, FA : Fils ainé, FC : Fils cadet, P : Père, etc 
    @recurs bit      -- La descendance est déplacé aussi 
)
AS
BEGIN 
	/****************************************************************************** 
	* PROCÉDURE DÉPLACEMENT D'UN SOUS ARBRE DANS UN ARBRE MODÉLISÉ PAR INTERVALLE * 
	******************************************************************************* 
	* Frédéric Brouard   -   SQLpro   -   http://www.sqlspot.com   -   2004-06-10 * 
	******************************************************************************* 
	* PARAMÈTRES :                                                                * 
	*    @id           clef de l'élément à déplacer (racine du sous arbre)        * 
	*    @id_parent    parent du point de déplacement (ancrage)                   * 
	*    @mode         parentèle au point d'ancrage. Valeurs possibles :          * 
	*                  GF : Frère (grand)                                         * 
	*                  PF : Frère (petit)                                         * 
	*                  FA : Fils ainé                                             * 
	*                  FC : Fils cadet                                            * 
	*                  P  : Père                                                  * 
	*    @recurs       si 1 déplacement de tout le sous arbre,                    * 
	*                  si 0 déplacement de l'élément seul                         * 
	******************************************************************************/ 
 
	DECLARE @OK     int, 
			@bdmax  int, -- limite supérieure droite de l'arborescence 
			@deltab int, -- Delta de borne 
			@bgd    int, -- Infos sur le déplacé 
			@bdd    int, 
			@nivd   int, 
			@bgp    int,    -- Infos sur le parent 
			@bdp    int, 
			@nivp   int 
 
	-- Mode silencieux 
	SET NOCOUNT ON; 
 
	-- contrôle d'exécution 
	IF @mode IS NULL 
	BEGIN 
	   RAISERROR ('Déplacement impossible sans mode ! (TABLE CK.tZone)', 16, 1) 
	   RETURN 
	END 
 
	IF @recurs IS NULL 
	BEGIN 
	   RAISERROR ('Déplacement impossible sans type ! (TABLE CK.tZone)', 16, 1) 
	   RETURN 
	END 
 
	IF @id IS NULL 
	BEGIN 
	   RAISERROR ('Déplacement impossible sans précision de l''élément ! (TABLE CK.tZone)', 16, 1) 
	   RETURN 
	END 
 
	IF @id_parent IS NULL 
	BEGIN 
	   RAISERROR ('Déplacement impossible sans précision du destinataire ! (TABLE CK.tZone)', 16, 1) 
	   RETURN 
	END 
 
	IF @id = @id_parent 
	BEGIN 
	   RAISERROR ('Déplacement impossible , origine et destination identique ! (TABLE CK.tZone)', 16, 1) 
	   RETURN 
	END 
 
	SET @mode = UPPER(@mode) 
	IF NOT( @mode = 'GF' OR @mode = 'PF' OR @mode = 'FC' OR @mode = 'FA' ) --  OR @mode = 'P' -- PBT: à vérifier ! 
	BEGIN 
	   RAISERROR ('Déplacement impossible, mode inconnu ! (TABLE CK.tZone)', 16, 1) 
	   RETURN 
	END 
     
	-- L'élément existe toujours ? 
	SELECT @OK = count(*) FROM CK.tZone WHERE ZoneId = @id 
	IF @OK IS NULL OR @OK = 0 
	BEGIN 
	   RAISERROR ('Déplacement impossible, l''élément n''existe pas ! (TABLE CK.tZone)', 16, 1) 
	   RETURN 
	END 
 
	-- Le parent existe toujours ? 
	SELECT @OK = count(*) FROM CK.tZone WHERE ZoneId = @id_parent 
	IF @OK IS NULL OR @OK = 0 
	BEGIN 
	   RAISERROR ('Déplacement impossible, le parent n''existe pas ! (TABLE CK.tZone)', 16, 1) 
	   RETURN 
	END 
 
	-- On récupère la borne supérieure max de l'ensemble 
	SELECT @bdmax = max(RightNumber) FROM CK.tZone 
	IF @bdmax IS NULL OR @bdmax = 0 
	BEGIN 
	   RAISERROR ('Déplacement impossible, la borne BD est nulle ! (TABLE CK.tZone)', 16, 1) 
	   RETURN 
	END 
 
	-- On a un élément : on récupère ses caractéristiques 
	SELECT @bgd = LeftNumber, @bdd = RightNumber, @nivd = Depth  
		   FROM CK.tZone  
		   WHERE ZoneId = @id 

	SET @deltab = @bdmax + 1 - @bgd 
 
	-- DEBUT de l'opération proprement dite... 
 
	-- Opération récursive ? 
	IF @recurs = 0 
	BEGIN 
	   -- Seul l'élément doit être déplacé 
 
	   -- déplacement de l'élément seul en fin de liste 
	   UPDATE CK.tZone 
			  SET LeftNumber = LeftNumber + @deltab, 
				  RightNumber = RightNumber + @deltab 
			  WHERE ZoneId = @id 
 
	   -- remontée de ses descendants dans la hiérarchie 
	   UPDATE CK.tZone 
			  SET LeftNumber = LeftNumber - 1, 
				  RightNumber = RightNumber - 1, 
				  Depth = Depth - 1 
			  WHERE LeftNumber > @bgd AND RightNumber < @bdd 

	   -- rééquilibrage de l'arbre 
	   UPDATE CK.tZone 
			  SET LeftNumber = LeftNumber - 2 
			  WHERE LeftNumber > @bdd AND LeftNumber < @bdmax 

	   --     
	   UPDATE CK.tZone 
			  SET RightNumber = RightNumber - 2 
			  WHERE RightNumber > @bdd AND RightNumber <= @bdmax 

	   -- Mise en place de l'élement deplacé 
 
	   -- On récupère d'abord les caractéristiques du parent 
	   SELECT @bgp = LeftNumber, @bdp = RightNumber, @nivp = Depth  
			  FROM CK.tZone  
			  WHERE ZoneId = @id_parent 

 
	   -- Fils cadet 
	   IF @mode = 'FC' 
	   BEGIN 
		  -- Limite sup. 
		  UPDATE CK.tZone 
				 SET RightNumber = RightNumber + 2 
				 WHERE RightNumber >= @bdp AND RightNumber <= @bdmax 

		  -- Limite inf. 
		  UPDATE CK.tZone 
				 SET LeftNumber = LeftNumber + 2 
				 WHERE LeftNumber > @bdp AND LeftNumber < @bdmax 
   
		  UPDATE CK.tZone 
				 SET LeftNumber = @bdp, 
					 RightNumber = @bdp + 1, 
					 Depth = @nivp + 1 
				 WHERE ZoneId = @id 

	   END 
 
	   -- Fils ainé 
	   IF @mode = 'FA' 
	   BEGIN 
		  -- Limite sup. 
		  UPDATE CK.tZone 
				 SET RightNumber = RightNumber + 2 
				 WHERE RightNumber > @bgp AND RightNumber <= @bdmax 

		  -- Limite inf. 
		  UPDATE CK.tZone 
				 SET LeftNumber = LeftNumber + 2 
				 WHERE LeftNumber > @bgp AND LeftNumber < @bdmax 

		  UPDATE CK.tZone 
				 SET LeftNumber = @bgp + 1, 
					 RightNumber = @bgp + 2, 
					 Depth = @nivp + 1 
				 WHERE ZoneId = @id 

	   END 
	END 
	ELSE 
	BEGIN 
	   -- L'élément et sa descendance doivent être déplacés 
 
	   -- déplacement de l'élément et de sa descendance en fin de liste 
	   -- Le niveau sera réactualisé ultérieurement 
	   UPDATE CK.tZone 
			  SET LeftNumber = LeftNumber + @deltab, 
				  RightNumber = RightNumber + @deltab, 
				  Depth = Depth - @nivd 
			  WHERE LeftNumber >= @bgd AND RightNumber <= @bdd 
 
	   -- rétablissement des bornes 
 
	   -- Calcul du Delta 
	   SET @deltab = @bdd - @bgd + 1 
	   -- 
	   UPDATE CK.tZone 
			  SET RightNumber = RightNumber - @deltab 
			  WHERE RightNumber > @bdd AND LeftNumber <= @bdmax 

	   --  
	   UPDATE CK.tZone 
			  SET LeftNumber = LeftNumber - @deltab 
			  WHERE LeftNumber > @bdd AND LeftNumber < @bdmax 

	   -- On récupère d'abord les caractéristiques du parent 
	   SELECT @bgp = LeftNumber, @bdp = RightNumber, @nivp = Depth  
			  FROM CK.tZone  
			  WHERE ZoneId = @id_parent 
 
	   -- Mise en place de l'élement deplacé
 
	   -- Fils cadet 
	   IF @mode = 'FC' 
	   BEGIN 
		  -- Limite sup. 
		  UPDATE CK.tZone 
				 SET RightNumber = RightNumber + @deltab 
				 WHERE RightNumber >= @bdp AND RightNumber <= @bdmax 

   
		  -- Limite inf. 
		  UPDATE CK.tZone 
				 SET LeftNumber = LeftNumber + @deltab 
				 WHERE LeftNumber > @bdp AND LeftNumber < @bdmax 

		  -- Recadrage de la liste d'éléments déplacés 
		  SET @deltab = @bdmax - @bdp + 1 
		  UPDATE CK.tZone 
				 SET LeftNumber = LeftNumber - @deltab, 
					 RightNumber = RightNumber - @deltab, 
					 Depth = Depth + @nivp + 1 
				 WHERE LeftNumber > @bdmax 
     
	 

	   END 
 
	   -- Fils ainé 
	   IF @mode = 'FA' 
	   BEGIN 
		  -- Limite sup. 
		  UPDATE CK.tZone 
				 SET RightNumber = RightNumber + @deltab 
				 WHERE RightNumber > @bgp AND RightNumber <= @bdmax 
     
   
		  -- Limite inf. 
		  UPDATE CK.tZone 
				 SET LeftNumber = LeftNumber + @deltab 
				 WHERE LeftNumber > @bgp AND LeftNumber < @bdmax 
  
 
		  -- Recadrage de la liste d'éléments déplacés 
		  SET @deltab = @bdmax - @bgp 
		  UPDATE CK.tZone 
				 SET LeftNumber = LeftNumber - @deltab, 
					 RightNumber = RightNumber - @deltab, 
					 Depth = Depth + @nivp + 1 
				 WHERE LeftNumber > @bdmax 
  


	   END 
	END 

	--<PostMove>
	update CK.tGroup set ZoneId = @id_parent where GroupId = @id
	--</PostMove>
end
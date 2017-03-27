-- SetupConfig:{}
-- Inline Table Value Function (ITVF) version to get the parent ResNames from a ResName.
-- This is the fastest implementation I can achieve.
create function CK.fResNamePrefixes( @ResName varchar(128) )
returns table -- with schemabinding
 return
		with E1(n) as ( select 1 union all select 1 union all select 1 union all select 1 union all
						select 1 union all select 1 union all select 1 union all select 1 union all 
						select 1 union all select 1 union all select 1 union all select 1
					 ),                          -- 12 rows
		    E2(n) as (select 1 from E1 a, E1 b), -- 12*12  = 144 rows
			T(n) as ( 
						select top(len(@ResName)) n = row_number() over (order by  (select null) desc)-1 from E2
					) 
		select	ParentLevel = row_number() over (order by  (select null) desc), 
				ParentPrefix = SUBSTRING(@ResName, 0, len(@ResName)-T.n ) collate Latin1_General_100_BIN2
			from T
			where len(@ResName) > 1 and SUBSTRING(@ResName, len(@ResName)-T.n, 1 ) = '.';

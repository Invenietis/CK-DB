-- SetupConfig: { } 
-- 
-- This view is an union all of selects injected by the different providers.
-- The first record is a fake one (filtered with an always false clause) that eases
-- the injection and describes the expected data model.
--
-- The LastUsed can be '0001-01-01 00:00:00' (that is equal to DateTime.MinValue or CK.Core.Util.DateTime.UtcMinValue) 
-- in this view if a user has been registered in the provider but has not actually logged in yet.
--
-- An exemple of a transformer (the one from Google provider):
--
--		-- SetupConfig: { "AddRequires": "Model.CK.UserGoogleTable" } 
--		-- 
--		create transformer on CK.vUserAuthProvider
--		as
--		begin
--			inject "
--			union all
--			select UserId, 'Google', LastLoginTime from CK.tUserGoogle where UserId > 0
--			" after first part {select};
--		end
--
create view CK.vUserAuthProvider( UserId, Scheme, LastUsed )
as 
	select	UserId = 0, Scheme = 'Scheme (ProviderName when IsMultiScheme = 0)', LastUsed = 'non null datetime2(2)' -- (provider table).LastLoginTime
		from CKCore.tSystem where 1 = 0;


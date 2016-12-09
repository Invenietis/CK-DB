--[beginscript]

declare @DefaultScopeSetId int;
exec CK.sAuthScopeSetCreate 1, N'openid', @ScopeSetIdResult = @DefaultScopeSetId output;
update CK.tUserGoogle set ScopeSetId = @DefaultScopeSetId where UserId = 0;

--[endscript]
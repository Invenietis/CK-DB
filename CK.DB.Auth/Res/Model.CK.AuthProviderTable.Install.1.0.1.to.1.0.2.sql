
-- "Logically" disabling provider at the database level has no 
-- real interest since we should handle actual login of users anyway:
-- this has to be handled at the application level.
-- This attribute is too ambiguous.
-- This was the previous comment on EnableProvider method:
--
--         /// Enables or disables a provider. 
--         /// Disabled provider must be handled by the application: since implementation can heavily differ
--         /// between them, that some of their capabilities may continue to be operational, and because of
--         /// race conditions from the user perspective, provider implementations MUST ignore this flag: 
--         /// authentication must always be honored, this MUST be only used by GUI to avoid the actual use of a provider.
-- 
alter table CK.tAuthProvider drop column IsEnabled;

-- Drops the useless procedure here.
drop procedure CK.sAuthProviderIsEnableSet;

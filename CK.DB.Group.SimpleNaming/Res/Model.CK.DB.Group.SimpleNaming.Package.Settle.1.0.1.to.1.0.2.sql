-- Migration for Administrators group (n°2).

-- Attemps to rename the group n°2 into "Administrators".
-- We may end up with a "Administrators (x)": this is correctly safe.
exec CK.sGroupGroupNameSet 1, 2, 'Administrators';



--[beginscript]

if OBJECT_ID('CK.sResDestroyByResNamePrefix') is not null drop procedure CK.sResDestroyByResNamePrefix;
if OBJECT_ID('CK.sResDestroyResNameChildren') is not null drop procedure CK.sResDestroyResNameChildren;
if OBJECT_ID('CK.sResDestroyWithResNameChildren') is not null drop procedure CK.sResDestroyWithResNameChildren;

--[endscript]
%error = ForceRequiredAddOn("Support_ToolBrick");

if (%error == $Error_AddOn_NotFound)
{
	//client needs the ToolBrick pack for this to work
	error("ERROR: Tool_Manipulator - required add-on Support_ToolBrick not found");
}
else
{
	exec("./Tool_Manipulator.cs");
}
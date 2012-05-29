if (!isObject(ManipulatorGui))
	exec("./ManipulatorGui.gui");

if(!$addedManipulatorGui)
{
	$remapDivision[$remapCount] = "Manipulator";
	$remapName[$remapCount] = "Open Manipulator Window";
	$remapCmd[$remapCount] = "ToggleManipulatorGui";
	$remapCount++;
	$addedManipulatorGui = 1;
}
	
//ServerCmds used
//ServerCmdGetManipulatorFilters(); //will make the server give ManipulatorFilters through ClientCmdAddManipulatorFilter(%name);
//ServerCmdAddMIF("filter_id, filter_arg"); //add include filter
//ServerCmdAddMEF("filter_id, filter_arg"); //add exclude filter

function ToggleManipulatorGui(%val)
{
	if (%val)
	{
		//if (ManipulatorGui.isAwake())
		//	canvas.popDialog(ManipulatorGui);
		//else
		if (!ManipulatorGui.isAwake())
			canvas.pushDialog(ManipulatorGui);
	}
}

function ManipulatorGui::onWake(%this)
{
	echo("at least get here");
	if (!$Manipulator::hasFilterList)
	{
		resetManipulator();
	}
	
}

function resetManipulator()
{
	manipFL.clear();
	manipIL.clear();
	manipEL.clear();
	$Manipulator::uniqueID = 0;
	nabuoArg.setText("");
	$Manipulator::numFilters = 0;
	commandToServer('GetManipulatorFilters');
	$Manipulator::hasFilterList = 1;
}

function ClientCmdResetManipList()
{
	resetManipulator();
}

function ClientCmdAddManipulatorFilter(%name)
{
	echo("Adding" SPC %name);
	$Manipulator::filters[$Manipulator::numFilters] = %name;
	manipFL.add(%name, $Manipulator::numFilters);
	$Manipulator::numFilters++;
}

function cleanAddFilter()
{
	manipArg.setText("");
}

function manipCancel()
{
	canvas.popDialog(ManipulatorGui);
	cleanAddFilter();
}

function manipApply()
{
	%incStr = "";
	%excStr = "";
	commandToServer('ResetManipulator');
	for (%i = 0; %i < manipIL.rowCount(); %i++)
	{
		%row = manipIL.getRowTextById(manipIL.getRowId(%i));
		%incStr = %incStr @ getField(%row, 2) TAB getField(%row, 1);
		commandToServer('AddMIF', getField(%row, 2), getField(%row, 1));
	}
	for (%e = 0; %e < manipEL.rowCount(); %e++)
	{
		%row = manipEL.getRowTextById(manipEL.getRowId(%e));
		%excStr = %excStr @ getField(%row, 2) TAB getField(%row, 1);
		commAndToServer('AddMEF', getField(%row, 2), getField(%row, 1));
	}
	
	canvas.popDialog(ManipulatorGui);
	//commandToServer('UpdateManipulator', %incStr, %excStr);
}

function manipInclude()
{
	manipIL.addRow($Manipulator::UniqueID++, $Manipulator::filters[manipFL.getSelected()] TAB manipArg.getValue() TAB manipFL.getSelected());
	cleanAddFilter();
}

function manipExclude()
{
	manipEL.addRow($Manipulator::UniqueID++, $Manipulator::filters[manipFL.getSelected()] TAB manipArg.getValue() TAB manipFL.getSelected());
	cleanAddFilter();
}

function manipRemoveInclude()
{
	manipIL.removeRow(manipIL.getRowNumById(manipIL.getSelectedId()));
}

function manipRemoveExclude()
{
	manipEL.removeRow(manipEL.getRowNumById(manipEL.getSelectedId()));
}

package manipulatorClient
{
	function disconnectedCleanup()
	{
		$Manipulator::hasFilterList = 0;
		Parent::disconnectedCleanup();
	}
};

activatePackage(manipulatorClient);
function manipulatorImage::onFire(%this, %obj)
{
	%types = ($TypeMasks::FxBrickAlwaysObjectType);
	%col = containerRaycast(%obj.getEyePoint(), VectorAdd(VectorScale(%obj.getEyeVector(), 8), %obj.getEyePoint()), %types);
	%col = getWord(%col, 0);
	echo("onFire" SPC %this SPC %obj SPC %col);
	manipulatorProjectile::onCollision(%this, %obj, %col);
}

datablock ItemData(manipulatorItem : wandItem)
{
	uiName = "Manipulator";
	doColorShift = true;
	colorShiftColor = "0.471 0.471 0.471 1.000";
	image = manipulatorImage;
};

datablock ShapeBaseImageData(manipulatorImage : wandImage)
{
	item = manipulatorItem;
	doColorShift = True;
	colorShiftColor = "0.471 0.471 0.471 1.000";
};

function manipulatorProjectile::onCollision(%this,%obj,%col)
{
	echo("wath" SPC %this SPC %obj SPC %col);
	%client = %obj.client;
	if (!(%client.isAdmin || %client.isSuperAdmin))
	{
		//commandToClient(%client, 'centerPrint', "\c2This tool is admin only.", 3);
		//return;
	}
	if (%client.vblMode $= "")
	{
		%client.vblMode = "Copy";
	}
	if (!isObject(%client.vbl))
		%client.vbl = newVBL();
	if (%client.vblMode $= "Place")
	{
		%client.player.tempbrick.setTransform(%col.getTransform());
		//commandToClient(%client, 'centerPrint', "\c2Use your tempbrick to place place the build or cancel brick.", 3);
	}
	else if (%col.getClassName() $= "fxDTSBrick" && %client.vblMode $= "Copy")
	{
		%bf = new ScriptObject()
		{
			class = "BrickFinder";
		};
		%bf.setOnSelectCommand(%client @ ".onManipulatorSelect(%sb);");
		%bf.setFinishCommand(%client @ ".onManipulatorDoneSearching(" @ %bf @ ");");
		%client.vblMode = "Copying";
		%client.vbl.clearList();
		commandToClient(%client, 'centerPrint', "\c2Build being copied, you are now in \c1Copying\c2 mode.", 3);
		for (%i = 0; %i < %client.manipNumIL; %i++)
			%incStr = %incStr @ %client.manipILs[%i] @ "\t";
		for (%e = 0; %e < %client.manipNumEL; %e++)
			%excStr = %excStr @ %client.manipELs[%e] @ "\t";
		%bf.search(%col, "chain", %incStr, %excStr, 1);
		//%client.vbl.importBuild(%col, 0, 1);
	}
	else if (%client.vblMode $= "Copy")
	{
		commandToClient(%client, 'centerPrint', "\c2Hit a baseplate to copy a build.", 3);
	}
	else if (%client.vblMode $= "Copying")
	{
		commandToClient(%client, 'centerPrint', "\c2Please wait, the manipulator is searching for bricks.", 3);
	}
	else
	{
		commandToClient(%client, 'centerPrint', "\c2This message should not appear! Please tell Nitramtj.", 3);
	}
}

function Gameconnection::onManipulatorSelect(%client, %brick)
{
	if (%client.vblMode $= "Copying")
	{
		%client.vbl.addRealBrick(%brick);
		if (!%brick.highlighted)
		{
			%brick.origColor = %brick.getColorId();
			%brick.setColor(3);
			%brick.highlighted = 1;
		}
	}
}

function Gameconnection::onManipulatorDoneSearching(%client, %bf)
{
	if (%client.vblMode !$= "Copying")
	{
		%bf.delete();
		return;
	}
	if (%bf.selectBricks.getCount() < 1)
	{
		%client.vblMode = "Copy";
		commandToClient(%client, 'centerPrint', "\c2No bricks were selected, your are now in \c1Copy\c2 mode.", 3);
		%bf.delete();
	}
	else
	{
		%sb = %bf.selectBricks.getObject(0);
		//for (%i = 0; %i < %bf.selectBricks.getCount(); %i++)
		//	%client.vbl.addRealBrick(%bf.selectBricks.getObject(%i));
		schedule(1000, 0, "highlightBricks", %bf.selectBricks, 1);
		%bf.schedule(1500, "delete");
		if (!isObject(%client.player.tempbrick))
		{
			%client.player.tempbrick = new fxDTSBrick()
			{
				datablock = %sb.getDataBlock();
			};
		}
		%tb = %client.player.tempbrick;
		%tb.setDataBlock(%sb.getDataBlock());
		%tb.setTransform(%sb.getTransform());
		%tb.isVblBase = true;
		%client.vblMode = "Place";
		commandToClient(%client, 'centerPrint', "\c2Build copied, you are now in \c1Place\c2 mode.", 3);
	}
}

function ServerCmdManipulator(%client)
{
	if (isObject(%client.player))
	{
		%client.player.updateArm(manipulatorImage);
		%client.player.mountImage(manipulatorImage, 0);
	}
}

package ManipulatorPackage
{
	//TODO: Move to Tool_Manipulator file
	function ServerCmdPlantBrick(%client)
	{
		if (isObject(%client.player) && isObject(%client.player.tempBrick) && %client.player.tempBrick.isVblBase)
		{
			%tb = %client.player.tempBrick;
			%pos = %tb.getPosition();
			%ad = %tb.getAngleId() - %client.vbl.virBricks[0, 2];
			if (%ad > 0) %client.vbl.rotateBricksCW(%ad);
			else %client.vbl.rotateBricksCCW(mAbs(%ad));
			%dif = VectorSub(%pos, %client.vbl.virBricks[0, 1]);
			%client.vbl.shiftBricks(%dif);
			%client.vbl.copyNum += 1;
			%client.vbl.createBricks();
			%client.vbl.shiftBricks(VectorScale(%dif, -1));
			//%client.vbl.clearList();
			//%client.vblMode = "Copy";
			//commandToClient(%client, 'centerPrint', "\c2Build placed, you have been set to \c1Copy\c2 mode.", 3);
			//%tb.isVblBase = false;
			//%tb.delete();
		}
		else
		{
			Parent::ServerCmdPlantBrick(%client);
		}
	}

	function fxDTSBrick::onRemove(%obj)
	{
		if (%obj.isVblBase)
		{
			for (%i = 0; %i < clientGroup.getCount(); %i++)
			{
				%client = clientGroup.getObject(%i);
				if (isObject(%client.player) && %client.player.tempBrick == %obj)
				{
					%client.vblMode = "Copy";
					commandToClient(%client, 'centerPrint', "\c2Build placement cancelled, you have been set to \c1Copy\c2 mode.", 3);
					break;
				}
			}
		}
		Parent::onRemove(%obj);
	}

	function fxDTSBrick::setDataBlock(%obj, %datablock)
	{
		if (%obj.isVblBase)
		{
			for (%i = 0; %i < clientGroup.getCount(); %i++)
			{
				%client = clientGroup.getObject(%i);
				if (isObject(%client.player) && %client.player.tempBrick == %obj)
				{
					%client.vblMode = "Copy";
					%obj.isVblBase = false;
					commandToClient(%client, 'centerPrint', "\c2Build placement cancelled, you have been set to \c1Copy\c2 mode.", 3);
					break;
				}
			}
		}
		Parent::setDataBlock(%obj, %datablock);
	}

	function ServerCmdLoadAllBricks(%client)
	{
		%client.vbl.loadBricks();
	}
};

activatePackage(ManipulatorPackage);
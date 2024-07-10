--mek retaynurs

--config. 
--you need navmesh, pandora, teleporter, lifestream, somethingeeddoing, textadvance, yesalready, simpletweaks
--and yesalready. setup yesalready to auto accept any purchases from npcs ;o
--also assumes limsa start and quests.

--simpletweaks config -> targeting fix
--snd -> turn off snd targeting

--Yesalready configs
-- -> will be lots once i get around to it

function visland_stop_moving()
 yield("/equipguud")
 yield("/equiprecommended")
 yield("/character")
 yield("/pcall Character true 15")
 yield("/wait 0.5")
 yield("/pcall SelectYesno true 0")
 yield("/character")
 yield("/pcall Character true 15")
 yield("/pcall SelectYesno true 0")
 yield("/wait 3")
 muuv = 1
 muuvX = GetPlayerRawXPos()
 muuvY = GetPlayerRawYPos()
 muuvZ = GetPlayerRawZPos()
 while muuv == 1 do
	yield("/wait 1")
	if muuvX == GetPlayerRawXPos() and muuvY == GetPlayerRawYPos() and muuvZ == GetPlayerRawZPos() then
		muuv = 0
	end
	muuvX = GetPlayerRawXPos()
	muuvY = GetPlayerRawYPos()
	muuvZ = GetPlayerRawZPos()
 end
 yield("/echo movement stopped safely - script proceeding to next bit")
 yield("/visland stop")
 yield("/vnavmesh stop")
 yield("/wait 3")
end

--buy 2 rods
yield("/vnavmesh moveto -246.67446899414 16.199998855591 41.268531799316")
visland_stop_moving()
yield("/target Syneyhil")
yield("/wait 2")
yield("/interact")
yield("/wait 2")
yield("/pcall SelectIconString true 1 <wait.2>")
yield("/pcall SelectString true 0 <wait.2>")
yield("/pcall Shop true 0 5 1 <wait.1.0>")
yield("/pcall Shop true 0 5 1 <wait.1.0>")
yield("/pcall Shop true -1 <wait.1.0>")
visland_stop_moving()

--get to vocate
yield("/vnavmesh moveto -146.021484375 18.212013244629 17.593742370605")
visland_stop_moving()
yield("/target Frydwyb")
yield("/wait 2")
yield("/interact")
yield("/wait 10") -- give it some time to reach the retainer screen.
--yield("/pcall _CharaMakeProgress true 0 1 0 Elezen 2")
yield("/pcall _CharaMakeProgress true -13 -1")
yield("/pcall _CharaMakeProgress true 0 0 0 Hyur 1")
yield("/pcall _CharaMakeProgress true -16 1")
--this is where it fails. it does not click the checkbox to continue. we stop for now ;p
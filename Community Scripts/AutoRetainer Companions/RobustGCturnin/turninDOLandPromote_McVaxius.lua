 yield("/tp Limsa Lominsa <wait.8>")
 yield("/waitaddon NamePlate <wait.1>")
 yield("/target Aetheryte <wait.1>")
 yield("/lockon")
 yield("/automove")
 yield("<wait.2>")

 yield("/pinteract <wait.1>")
 yield("/pcall SelectString true 0")
 yield("/pcall TelepotTown false 11 1u")
 yield("/pcall TelepotTown false 11 1u")

 yield("<wait.5>")
 yield("/ac Sprint")
 yield("/ac Sprint")
 yield("/ac Sprint")
 yield("/visland execonce GClimsa")
 yield("/wait 1") --simple tweaks
 --/tweaks
 --auto equip -> alias /equipguud
 yield("/equipguud")
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

 yield("/echo movement stopped - time for GC turn ins")
--yield("<wait.15>")
--yield("/waitaddon SelectString <maxwait.120>")
 yield("/visland stop")
yield("/wait 1")
yield("/target Personnel Officer")
yield("/wait 1")
yield("/send NUMPAD0")
yield("/pcall SelectString true 0 <wait.1>")
yield("/send NUMPAD0")
yield("/wait 1")
yield("/send NUMPAD0")
yield("/wait 1")
yield("/pcall GrandCompanySupplyList true 0 1 2")
yield("/wait 1")
yield("/send NUMPAD0")
yield("/wait 1")
yield("/send NUMPAD0")
yield("/wait 1")
yield("/send ESCAPE <wait.1.5>")
yield("/send ESCAPE <wait.1.5>")
yield("/wait 3")

--try to promote
yield("/wait 1")
yield("/target Personnel Officer")
yield("/wait 1")
yield("/send NUMPAD0")
yield("/wait 1")
yield("/send NUMPAD2")
yield("/wait 0.5")
yield("/send NUMPAD0")
yield("/wait 0.5")
yield("/send NUMPAD0")
yield("/send ESCAPE <wait.1.5>")
yield("/send ESCAPE <wait.1.5>")
yield("/wait 3")


-- Teleport back to FC House
yield("/tp Estate Hall <wait.10>")
yield("/waitaddon _ActionBar <maxwait.600><wait.5>")
for i=1, 20 do
  yield("/send W")
end

yield("/send Right")
yield("/send Right")


yield("/target Entrance <wait.1>")
yield("/lockon on")
yield("/automove on <wait.2.5>")
yield("/automove off <wait.1.5>")
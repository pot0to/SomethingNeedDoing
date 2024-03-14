function WalkTo(x, y, z)
    PathfindAndMoveTo(x, y, z, false)
    while (PathIsRunning() or PathfindInProgress()) do
        yield("/wait 0.5")
    end
end

function ZoneTransition()
    repeat 
        yield("/wait 0.5")
        yield("/echo Are we ready?")
    until not IsPlayerAvailable()
    repeat 
        yield("/wait 0.5")
        yield("/echo Are we ready? (backup check)")
    until IsPlayerAvailable()
end

function WalkToGC()
    if GetPlayerGC() == 1 then
        yield("/li The Aftcastle")
        ZoneTransition()
        WalkTo(94, 40.5, 74.5)
    elseif GetPlayerGC() == 2 then
        WalkTo(-68.5, -0.5, -8.5)
    elseif GetPlayerGC() == 3 then
        WalkTo(-142.5, 4, -106.5)
    end
end

function TargetedInteract(target)
    yield("/target "..target.." <wait.0.5>")
    yield("/pinteract <wait.1>")
end

function DidWeLoadcorrectly()
	yield("/echo We loaded the functions file successfully!")
end

function CharacterSafeWait()
     yield("/echo 15 second wait for char swap")
	 yield("/wait 15")
	 yield("/waitaddon NamePlate <maxwait.600> <wait.5>")

end
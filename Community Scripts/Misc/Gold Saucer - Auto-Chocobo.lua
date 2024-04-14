justgo = true
zone = GetZoneID()

while justgo do
    repeat
        zone = GetZoneID()
    until zone == 388

    repeat
        yield("/dutyfinder <wait.1>")
    until IsAddonReady("ContentsFinder")
    yield("/pcall ContentsFinder true 12 0")

    repeat
        zone = GetZoneID()
        yield("/wait 1")
    until zone ~= 388

    counter = 0
    repeat
        yield("/hold W")
        counter = counter + 1
        if counter == 15 or
            counter == 30 or
            counter == 45 or
            counter == 60 or
            counter == 75 or
            counter == 91 or
            counter == 105 or
            counter == 120 or
            counter == 135 then
            yield("/send KEY_1")
        end
        if counter == 90 then
            yield("/send KEY_2")
        end
        yield("/wait 1")
    until IsAddonReady("RaceChocoboResult")

    yield("/wait 7")
    yield("/e Click!")
    yield("/pcall RaceChocoboResult true 1 0 <wait.1>")
    yield("/release W")
end

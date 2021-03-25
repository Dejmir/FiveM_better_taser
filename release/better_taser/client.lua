RegisterNetEvent('better_taser:nui:lua')
AddEventHandler('better_taser:nui:lua', function()
    SendNUIMessage({type = 'open'})
	SetNuiFocus(true, true)
end)

RegisterNetEvent('better_taser:nui:lua:cartridge')
AddEventHandler('better_taser:nui:lua:cartridge', function(var)
    SendNUIMessage({type = var})
end)
RegisterNetEvent('better_taser:nui:lua:batteries')
AddEventHandler('better_taser:nui:lua:batteries', function(var)
    SendNUIMessage({type = var})
end)

RegisterNetEvent('better_taser:nui:off')
AddEventHandler('better_taser:nui:off', function()
	closeui()
end)

RegisterNUICallback('maincallback', function(data, cb)
	local callback = data.callback;
	--Used for debug
	--TriggerEvent('chat:addMessage', {
	--	color = { 255, 0, 0},
	--	multiline = false,
	--	args = {callback}
	--})
	if callback == "quit" then TriggerEvent("better_taser:nui:off") end
	if callback == "cartridge" then TriggerEvent("better_taser:cartridge", "check") closeui() end
	if callback == "batteries" then TriggerEvent("better_taser:batteries") closeui() end
	if callback == "destroy" then closeui() end
end)

function closeui()
	SetNuiFocus(false, false)
end
local world_panel = {}

world_panel.name = "PrismaticHelper/WorldPanel"
world_panel.fillColor = {0.5, 0.4, 0.6, 0.6}
world_panel.borderColor = {0.2, 0.15, 0.25, 0.4}
world_panel.placements = {
      name = "world_panel",
      data = {
          width = 8,
          height = 8,
          foreground = true,
          scrollX = 1,
          scrollY = 1,
          opacity = 1,
          room = "a-01",
          tint = "#ffffff",
          image = "",
          mask = "",
          attached = false,
          name = ""
      }
}

function world_panel.depth(room, entity)
    return entity.foreground and -12501 or 10001
end

return world_panel
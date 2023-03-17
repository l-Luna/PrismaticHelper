local windowpane = {}

windowpane.name = "PrismaticHelper/Windowpane"
windowpane.fillColor = {0.5, 0.4, 0.6, 0.6}
windowpane.borderColor = {0.2, 0.15, 0.25, 0.4}
windowpane.placements = {
      name = "windowpane",
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

return windowpane
--return nil
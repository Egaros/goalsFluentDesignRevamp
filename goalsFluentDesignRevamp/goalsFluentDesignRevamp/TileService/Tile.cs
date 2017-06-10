using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using NotificationsExtensions.Tiles;
using NotificationsExtensions;
using Windows.UI.StartScreen;

namespace goalsFluentDesignRevamp.TileService
{
    class tile
    {
        public async static void makeOrUpdateGoalTile(string name, string tileID, string progress, string description, string imagePath)
        {
            string percentage = progress.Remove(0, 9);
            string tileIDtoUse = tileID;
            // Construct the tile content
            var content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    Branding = TileBranding.NameAndLogo,
                    DisplayName = name,
                    TileSmall = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveText()
                    {
                        Text = percentage,
                        HintStyle = AdaptiveTextStyle.Base
                    }
                },
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = imagePath
                            }
                        }
                    },
                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveText()
                    {
                        Text = "Progress:",
                        HintStyle = AdaptiveTextStyle.Base
                    },
                    new AdaptiveText()
                    {
                        Text = percentage,
                        HintStyle = AdaptiveTextStyle.Title
                    }
                },
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = imagePath,
                                HintOverlay = 20
                            }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveGroup()
                    {
                        Children =
                        {
                            new AdaptiveSubgroup()
                            {
                                HintWeight = 33,
                                Children =
                                {
                                    new AdaptiveImage()
                                    {
                                        Source = imagePath
                                    }
                                }
                            },
                            new AdaptiveSubgroup()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = progress,
                                        HintStyle = AdaptiveTextStyle.Caption
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = description,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true,
                                        HintMaxLines = 3
                                    }
                                }
                            }
                        }
                    }
                }
                        }
                    },
                    TileLarge = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveGroup()
                    {
                        Children =
                        {
                            new AdaptiveSubgroup()
                            {
                                HintWeight = 50,
                                Children =
                                {
                                    new AdaptiveImage()
                                    {
                                        Source = imagePath
                                    }
                                }
                            },
                            new AdaptiveSubgroup()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = "Progress:",
                                        HintStyle = AdaptiveTextStyle.Caption
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = percentage,
                                        HintStyle = AdaptiveTextStyle.Subtitle
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveText()
                    {
                        Text = description,
                        HintStyle = AdaptiveTextStyle.Body,
                        HintWrap = true,
                        HintMaxLines = 3
                    }
                }
                        }
                    }
                }
            };

            var notification = new TileNotification(content.GetXml());

            // And send the notification
            if (SecondaryTile.Exists(tileIDtoUse))
            {
                // Get its updater
                var updater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileIDtoUse);
                // And send the notification
                updater.Update(notification);
            }
            else
            {
                SecondaryTile tile = GenerateSecondaryTile(tileIDtoUse, name);
               bool allowedToPin = await tile.RequestCreateAsync();
                // Get its updater
                if (allowedToPin)
                {
                var updater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileIDtoUse);

                // And send the notification
                updater.Update(notification);
                }
            }



        }

        internal static bool checkIfTileIsPinned(string tileID)
        {
            bool isPinned = SecondaryTile.Exists(tileID);
            return isPinned;
        }

        private static SecondaryTile GenerateSecondaryTile(string tileId, string displayName)
        {
            SecondaryTile tile = new SecondaryTile(tileId, displayName, "args", new Uri("ms-appx:///Assets/Logo.png"), TileSize.Default);
            tile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/Small.png");
            tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Wide.png");
            tile.VisualElements.Square310x310Logo = new Uri("ms-appx:///Assets/Large.png");
            tile.VisualElements.Square44x44Logo = new Uri("ms-appx:///Assets/Square44x44Logo.png"); // Branding logo
            return tile;
        }

        public static void updateExistingTile(string name, string progress, string description, string imagePath, string tileID)
        {
            string percentage = progress.Remove(0, 9);
            string tileIDtoUse = tileID;
            // Construct the tile content
            var content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    Branding = TileBranding.NameAndLogo,
                    DisplayName = name,
                    TileSmall = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveText()
                    {
                        Text = percentage,
                        HintStyle = AdaptiveTextStyle.Base
                    }
                },
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = imagePath
                            }
                        }
                    },
                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveText()
                    {
                        Text = "Progress:",
                        HintStyle = AdaptiveTextStyle.Base
                    },
                    new AdaptiveText()
                    {
                        Text = percentage,
                        HintStyle = AdaptiveTextStyle.Title
                    }
                },
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = imagePath,
                                HintOverlay = 20
                            }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveGroup()
                    {
                        Children =
                        {
                            new AdaptiveSubgroup()
                            {
                                HintWeight = 33,
                                Children =
                                {
                                    new AdaptiveImage()
                                    {
                                        Source = imagePath
                                    }
                                }
                            },
                            new AdaptiveSubgroup()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = progress,
                                        HintStyle = AdaptiveTextStyle.Caption
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = description,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true,
                                        HintMaxLines = 3
                                    }
                                }
                            }
                        }
                    }
                }
                        }
                    },
                    TileLarge = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveGroup()
                    {
                        Children =
                        {
                            new AdaptiveSubgroup()
                            {
                                HintWeight = 50,
                                Children =
                                {
                                    new AdaptiveImage()
                                    {
                                        Source = imagePath
                                    }
                                }
                            },
                            new AdaptiveSubgroup()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = "Progress:",
                                        HintStyle = AdaptiveTextStyle.Caption
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = percentage,
                                        HintStyle = AdaptiveTextStyle.Subtitle
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveText()
                    {
                        Text = description,
                        HintStyle = AdaptiveTextStyle.Body,
                        HintWrap = true,
                        HintMaxLines = 3
                    }
                }
                        }
                    }
                }
            };

            
                var notification = new TileNotification(content.GetXml());
                var updater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileIDtoUse);
                updater.Update(notification);

        }
        
    }

}


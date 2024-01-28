using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Scenes.SettingsScene;
using _Scripts.Scenes.SettingsScene.UI;
using _Scripts.Technical;
using UnityEngine;

namespace _Scripts.ProjectContext.Managers.Settings
{
    public class GraphicsManager : MonoBehaviour
    {
        private readonly Dictionary<SettingsBlock.SettingsParamValues, int> _textureQualityMap =
            new()
            {
                {SettingsBlock.SettingsParamValues.Medium, 1},
                {SettingsBlock.SettingsParamValues.High, 0},
            };

        private readonly
            Dictionary<SettingsBlock.SettingsParamValues, (ShadowQuality quality, ShadowResolution resolution)>
            _shadowQualityMap =
                new()
                {
                    {SettingsBlock.SettingsParamValues.Off, (ShadowQuality.Disable, ShadowResolution.Low)},
                    {SettingsBlock.SettingsParamValues.Low, (ShadowQuality.HardOnly, ShadowResolution.Low)},
                    {SettingsBlock.SettingsParamValues.Medium, (ShadowQuality.All, ShadowResolution.Medium)},
                    {SettingsBlock.SettingsParamValues.High, (ShadowQuality.All, ShadowResolution.High)},
                    {SettingsBlock.SettingsParamValues.Ultra, (ShadowQuality.All, ShadowResolution.VeryHigh)}
                };

        private readonly Dictionary<SettingsBlock.SettingsParamValues, int> _antiAliasingMap =
            new()
            {
                {SettingsBlock.SettingsParamValues.Off, 0},
                {SettingsBlock.SettingsParamValues.Low, 2},
                {SettingsBlock.SettingsParamValues.Medium, 4},
                {SettingsBlock.SettingsParamValues.High, 8},
            };

        private readonly Dictionary<SettingsBlock.SettingsParamValues, bool> _vSyncMap =
            new()
            {
                {SettingsBlock.SettingsParamValues.Off, false},
                {SettingsBlock.SettingsParamValues.On, true},
            };

        private void Awake()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            LoadSettingIfExists(SaveKeys.TextureQuality, SetTextureQuality);
            LoadSettingIfExists(SaveKeys.ShadowsQuality, SetShadowsQuality);
            LoadSettingIfExists(SaveKeys.AntiAliasing, SetAntiAliasing);
            LoadSettingIfExists(SaveKeys.VSync, SetVSync);
            LoadSettingIfExists(SaveKeys.Resolution, SetResolution);
        }

        private void LoadSettingIfExists(string settingKey, Action<SettingsBlock.SettingsParamValues> setSettingValue)
        {
            if (!ES3.KeyExists(settingKey))
            {
                LoadDefaultSetting(settingKey, setSettingValue);
                return;
            }

            var settingValue = ES3.Load<string>(settingKey);
            var parsedSettingValue = Enum.Parse<SettingsBlock.SettingsParamValues>(settingValue);
            setSettingValue(parsedSettingValue);
        }

        private void LoadDefaultSetting(string settingKey, Action<SettingsBlock.SettingsParamValues> setSettingValue)
        {
            print($"Setting {settingKey} not found. Loading default value.");
            if (Enum.TryParse<SettingsBlock.SettingsParamType>(settingKey, out var settings))
            {
                var defaultSettingValue = SettingsSceneManager.DefaultSettings[settings];
                switch (settings)
                {
                    case SettingsBlock.SettingsParamType.TextureQuality:
                        var textureQuality =
                            (SettingsBlock.SettingsParamValues) Enum.Parse(typeof(SettingsBlock.SettingsParamValues),
                                defaultSettingValue);
                        setSettingValue(textureQuality);
                        break;
                    case SettingsBlock.SettingsParamType.ShadowsQuality:
                        var shadowsQuality =
                            (SettingsBlock.SettingsParamValues) Enum.Parse(typeof(SettingsBlock.SettingsParamValues),
                                defaultSettingValue);
                        setSettingValue(shadowsQuality);
                        break;
                    case SettingsBlock.SettingsParamType.AntiAliasing:
                        var antiAliasing =
                            (SettingsBlock.SettingsParamValues) Enum.Parse(typeof(SettingsBlock.SettingsParamValues),
                                defaultSettingValue);
                        setSettingValue(antiAliasing);
                        break;
                    case SettingsBlock.SettingsParamType.VSync:
                        var vSync = (SettingsBlock.SettingsParamValues) Enum.Parse(
                            typeof(SettingsBlock.SettingsParamValues), defaultSettingValue);
                        setSettingValue(vSync);
                        break;
                    case SettingsBlock.SettingsParamType.Resolution:
                        var screenResolution = Screen.resolutions.Last();
                        var resolution = $"_{screenResolution.width}x{screenResolution.height}";
                        if (Enum.TryParse(resolution, out SettingsBlock.SettingsParamValues resolutionValue))
                        {
                            setSettingValue(resolutionValue);
                        }
                        else
                        {
                            resolutionValue = (SettingsBlock.SettingsParamValues) Enum.Parse(
                                typeof(SettingsBlock.SettingsParamValues), defaultSettingValue);
                            setSettingValue(resolutionValue);
                        }

                        break;
                    case SettingsBlock.SettingsParamType.SoundVolume:
                    case SettingsBlock.SettingsParamType.MusicVolume:
                    case SettingsBlock.SettingsParamType.MouseSensitivity:
                    case SettingsBlock.SettingsParamType.InvertYAxis:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(settings), settings, null);
                }
            }
        }

        public void SetResolution(SettingsBlock.SettingsParamValues obj)
        {
            var str = obj.ToString();
            str = str.Remove(0, 1);
            var split = str.Split('x');
            var width = int.Parse(split[0]);
            var height = int.Parse(split[1]);
            Screen.SetResolution(width, height, Screen.fullScreen);
            ES3.Save(SaveKeys.Resolution, obj.ToString());
        }

        public void SetTextureQuality(SettingsBlock.SettingsParamValues textureQuality)
        {
            if (_textureQualityMap.TryGetValue(textureQuality, out var value))
            {
                QualitySettings.globalTextureMipmapLimit = value;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(textureQuality), textureQuality, null);
            }

            ES3.Save(SaveKeys.TextureQuality, textureQuality.ToString());
        }

        public void SetShadowsQuality(SettingsBlock.SettingsParamValues shadowsQuality)
        {
            if (_shadowQualityMap.TryGetValue(shadowsQuality, out var value))
            {
                QualitySettings.shadows = value.quality;
                QualitySettings.shadowResolution = value.resolution;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(shadowsQuality), shadowsQuality, null);
            }

            ES3.Save(SaveKeys.ShadowsQuality, shadowsQuality.ToString());
        }

        public void SetAntiAliasing(SettingsBlock.SettingsParamValues antiAliasing)
        {
            if (_antiAliasingMap.TryGetValue(antiAliasing, out var value))
            {
                QualitySettings.antiAliasing = value;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(antiAliasing), antiAliasing, null);
            }

            ES3.Save(SaveKeys.AntiAliasing, antiAliasing.ToString());
        }

        public void SetVSync(SettingsBlock.SettingsParamValues vSync)
        {
            if (_vSyncMap.TryGetValue(vSync, out var value))
            {
                QualitySettings.vSyncCount = value ? 1 : 0;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(vSync), vSync, null);
            }

            ES3.Save(SaveKeys.VSync, vSync.ToString());
        }

        public SettingsBlock.SettingsParamValues GetTextureQuality()
        {
            // Invert dictionary for reverse lookup.
            var invertedMap = _textureQualityMap.ToDictionary(pair => pair.Value, pair => pair.Key);
            // Check and return value. Note, implementation depends on how you stored these settings.
            if (invertedMap.TryGetValue(QualitySettings.globalTextureMipmapLimit, out var quality))
            {
                return quality;
            }

            throw new NotImplementedException("This graphic setting is not implemented.");
        }

        public SettingsBlock.SettingsParamValues GetShadowsQuality()
        {
            // Invert dictionary for reverse lookup.
            var invertedMap = _shadowQualityMap.ToDictionary(pair => pair.Value, pair => pair.Key);
            // Check and return value. Note, implementation depends on how you stored these settings.
            if (invertedMap.TryGetValue((QualitySettings.shadows, QualitySettings.shadowResolution), out var quality))
            {
                return quality;
            }

            throw new NotImplementedException("This graphic setting is not implemented.");
        }

        public SettingsBlock.SettingsParamValues GetAntiAliasing()
        {
            // Invert dictionary for reverse lookup.
            var invertedMap = _antiAliasingMap.ToDictionary(pair => pair.Value, pair => pair.Key);
            // Check and return value. Note, implementation depends on how you stored these settings.
            if (invertedMap.TryGetValue(QualitySettings.antiAliasing, out var quality))
            {
                return quality;
            }

            throw new NotImplementedException("This graphic setting is not implemented.");
        }

        public SettingsBlock.SettingsParamValues GetVSync()
        {
            // Invert dictionary for reverse lookup.
            var invertedMap = _vSyncMap.ToDictionary(pair => pair.Value, pair => pair.Key);
            // Check and return value. Note, implementation depends on how you stored these settings.
            if (invertedMap.TryGetValue(QualitySettings.vSyncCount == 1, out var quality))
            {
                return quality;
            }

            throw new NotImplementedException("This graphic setting is not implemented.");
        }

        public Resolution GetResolution()
        {
            return Screen.currentResolution;
        }
    }
}
using Godot;
using System.Collections.Generic;

namespace PlantsVsZombies.Core
{
    /// <summary>
    /// 音频管理器 - 统一管理游戏中的音效和背景音乐
    /// </summary>
    public partial class AudioManager : Node
    {
        public static AudioManager Instance { get; private set; }

        // 音量设置
        [Export] public float MasterVolume { get; set; } = 1.0f;
        [Export] public float MusicVolume { get; set; } = 0.7f;
        [Export] public float SfxVolume { get; set; } = 0.8f;

        // 音频播放器
        private AudioStreamPlayer _musicPlayer;
        private Dictionary<string, AudioStreamPlayer> _sfxPlayers = new Dictionary<string, AudioStreamPlayer>();
        private AudioStreamPlayer2D _sfxPlayer2D; // 用于2D音效定位

        // 音频资源
        private Dictionary<string, AudioStream> _musicTracks = new Dictionary<string, AudioStream>();
        private Dictionary<string, AudioStream> _soundEffects = new Dictionary<string, AudioStream>();

        // 当前状态
        private string _currentMusicTrack = "";
        private bool _isMusicMuted = false;
        private bool _isSfxMuted = false;

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                ProcessMode = ProcessModeEnum.Always;
                InitializeAudioPlayers();
                LoadAudioResources();
                SetupAudioBus();
            }
            else
            {
                QueueFree();
            }
        }

        /// <summary>
        /// 初始化音频播放器
        /// </summary>
        private void InitializeAudioPlayers()
        {
            // 创建背景音乐播放器
            _musicPlayer = new AudioStreamPlayer();
            _musicPlayer.Name = "MusicPlayer";
            _musicPlayer.Bus = "Music";
            AddChild(_musicPlayer);

            // 创建2D音效播放器
            _sfxPlayer2D = new AudioStreamPlayer2D();
            _sfxPlayer2D.Name = "SfxPlayer2D";
            _sfxPlayer2D.Bus = "SFX";
            AddChild(_sfxPlayer2D);

            // 创建通用音效播放器池
            for (int i = 0; i < 8; i++)
            {
                var player = new AudioStreamPlayer();
                player.Name = $"SfxPlayer_{i}";
                player.Bus = "SFX";
                player.Finished += () => OnSfxPlayerFinished(player);
                AddChild(player);
                _sfxPlayers.Add($"SfxPlayer_{i}", player);
            }

            GD.Print("[AudioManager] 音频播放器初始化完成");
        }

        /// <summary>
        /// 加载音频资源
        /// </summary>
        private void LoadAudioResources()
        {
            // 加载背景音乐
            LoadMusicTrack("main_menu", "res://Assets/Audio/Music/main_menu.ogg");
            LoadMusicTrack("gameplay", "res://Assets/Audio/Music/gameplay.ogg");
            LoadMusicTrack("credits", "res://Assets/Audio/Music/credits.ogg");

            // 加载音效
            LoadSoundEffect("plant", "res://Assets/Audio/SFX/plant.wav");
            LoadSoundEffect("sun_collect", "res://Assets/Audio/SFX/sun_collect.wav");
            LoadSoundEffect("pea_shoot", "res://Assets/Audio/SFX/pea_shoot.wav");
            LoadSoundEffect("explosion", "res://Assets/Audio/SFX/explosion.wav");
            LoadSoundEffect("zombie_groan", "res://Assets/Audio/SFX/zombie_groan.wav");
            LoadSoundEffect("cherry_bomb", "res://Assets/Audio/SFX/cherry_bomb.wav");
            LoadSoundEffect("button_click", "res://Assets/Audio/SFX/button_click.wav");
            LoadSoundEffect("card_select", "res://Assets/Audio/SFX/card_select.wav");
            LoadSoundEffect("warning", "res://Assets/Audio/SFX/warning.wav");

            GD.Print("[AudioManager] 音频资源加载完成");
        }

        /// <summary>
        /// 设置音频总线
        /// </summary>
        private void SetupAudioBus()
        {
            // 创建音频总线
            var masterBusIndex = AudioServer.GetBusIndex("Master");
            var musicBusIndex = AudioServer.GetBusCount();
            var sfxBusIndex = AudioServer.GetBusCount() + 1;

            AudioServer.AddBus();
            AudioServer.SetBusName(musicBusIndex, "Music");
            AudioServer.AddBus();
            AudioServer.SetBusName(sfxBusIndex, "SFX");

            // 设置总线音量
            AudioServer.SetBusVolumeDb(musicBusIndex, Mathf.LinearToDb(MusicVolume));
            AudioServer.SetBusVolumeDb(sfxBusIndex, Mathf.LinearToDb(SfxVolume));
        }

        /// <summary>
        /// 加载背景音乐轨道
        /// </summary>
        private void LoadMusicTrack(string name, string path)
        {
            var stream = GD.Load<AudioStream>(path);
            if (stream != null)
            {
                _musicTracks[name] = stream;
                GD.Print($"[AudioManager] 加载音乐轨道: {name}");
            }
            else
            {
                GD.Print($"[AudioManager] 警告: 无法加载音乐轨道 {name} 从路径 {path}");
            }
        }

        /// <summary>
        /// 加载音效
        /// </summary>
        private void LoadSoundEffect(string name, string path)
        {
            var stream = GD.Load<AudioStream>(path);
            if (stream != null)
            {
                _soundEffects[name] = stream;
                GD.Print($"[AudioManager] 加载音效: {name}");
            }
            else
            {
                GD.Print($"[AudioManager] 警告: 无法加载音效 {name} 从路径 {path}");
            }
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        public void PlayMusic(string trackName, bool loop = true)
        {
            if (!_musicTracks.ContainsKey(trackName))
            {
                GD.Print($"[AudioManager] 错误: 找不到音乐轨道 {trackName}");
                return;
            }

            if (_currentMusicTrack == trackName && _musicPlayer.Playing)
            {
                return; // 已经在播放相同的音乐
            }

            _currentMusicTrack = trackName;
            _musicPlayer.Stream = _musicTracks[trackName];

            if (_musicPlayer.Stream is AudioStreamOggVorbis oggStream)
            {
                oggStream.Loop = loop;
            }

            if (!_isMusicMuted)
            {
                _musicPlayer.Play();
            }

            GD.Print($"[AudioManager] 播放背景音乐: {trackName}");
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        public void StopMusic()
        {
            _musicPlayer.Stop();
            _currentMusicTrack = "";
            GD.Print("[AudioManager] 停止背景音乐");
        }

        /// <summary>
        /// 播放音效（通用）
        /// </summary>
        public void PlaySFX(string sfxName, float pitchScale = 1.0f)
        {
            if (!_soundEffects.ContainsKey(sfxName) || _isSfxMuted)
            {
                return;
            }

            // 查找空闲的音效播放器
            var player = GetAvailableSfxPlayer();
            if (player != null)
            {
                player.Stream = _soundEffects[sfxName];
                player.PitchScale = pitchScale;
                player.Play();

                GD.Print($"[AudioManager] 播放音效: {sfxName}");
            }
        }

        /// <summary>
        /// 播放2D音效（带位置）
        /// </summary>
        public void PlaySFX2D(string sfxName, Vector2 position, float pitchScale = 1.0f)
        {
            if (!_soundEffects.ContainsKey(sfxName) || _isSfxMuted)
            {
                return;
            }

            _sfxPlayer2D.Stream = _soundEffects[sfxName];
            _sfxPlayer2D.Position = position;
            _sfxPlayer2D.PitchScale = pitchScale;
            _sfxPlayer2D.Play();

            GD.Print($"[AudioManager] 播放2D音效: {sfxName} 在位置 {position}");
        }

        /// <summary>
        /// 获取可用的音效播放器
        /// </summary>
        private AudioStreamPlayer GetAvailableSfxPlayer()
        {
            foreach (var kvp in _sfxPlayers)
            {
                if (!kvp.Value.Playing)
                {
                    return kvp.Value;
                }
            }
            return null; // 所有播放器都在使用
        }

        /// <summary>
        /// 音效播放器播放完成回调
        /// </summary>
        private void OnSfxPlayerFinished(AudioStreamPlayer player)
        {
            // 音效播放完成，播放器可以被重用
            GD.Print($"[AudioManager] 音效播放完成: {player.Name}");
        }

        /// <summary>
        /// 设置主音量
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp(volume, 0f, 1f);
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), Mathf.LinearToDb(MasterVolume));
        }

        /// <summary>
        /// 设置音乐音量
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp(volume, 0f, 1f);
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), Mathf.LinearToDb(MusicVolume));
        }

        /// <summary>
        /// 设置音效音量
        /// </summary>
        public void SetSfxVolume(float volume)
        {
            SfxVolume = Mathf.Clamp(volume, 0f, 1f);
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), Mathf.LinearToDb(SfxVolume));
        }

        /// <summary>
        /// 切换音乐静音状态
        /// </summary>
        public void ToggleMusicMute()
        {
            _isMusicMuted = !_isMusicMuted;

            if (_isMusicMuted)
            {
                _musicPlayer.Stop();
            }
            else if (!string.IsNullOrEmpty(_currentMusicTrack))
            {
                _musicPlayer.Play();
            }

            GD.Print($"[AudioManager] 音乐静音: {_isMusicMuted}");
        }

        /// <summary>
        /// 切换音效静音状态
        /// </summary>
        public void ToggleSfxMute()
        {
            _isSfxMuted = !_isSfxMuted;

            if (_isSfxMuted)
            {
                // 停止所有正在播放的音效
                foreach (var player in _sfxPlayers.Values)
                {
                    if (player.Playing)
                    {
                        player.Stop();
                    }
                }
                _sfxPlayer2D.Stop();
            }

            GD.Print($"[AudioManager] 音效静音: {_isSfxMuted}");
        }

        /// <summary>
        /// 获取音量设置
        /// </summary>
        public AudioSettings GetAudioSettings()
        {
            return new AudioSettings
            {
                MasterVolume = MasterVolume,
                MusicVolume = MusicVolume,
                SfxVolume = SfxVolume,
                IsMusicMuted = _isMusicMuted,
                IsSfxMuted = _isSfxMuted
            };
        }

        /// <summary>
        /// 应用音量设置
        /// </summary>
        public void ApplyAudioSettings(AudioSettings settings)
        {
            SetMasterVolume(settings.MasterVolume);
            SetMusicVolume(settings.MusicVolume);
            SetSfxVolume(settings.SfxVolume);

            if (settings.IsMusicMuted != _isMusicMuted)
            {
                ToggleMusicMute();
            }

            if (settings.IsSfxMuted != _isSfxMuted)
            {
                ToggleSfxMute();
            }
        }

        /// <summary>
        /// 清理音频资源
        /// </summary>
        public void Cleanup()
        {
            StopMusic();

            foreach (var player in _sfxPlayers.Values)
            {
                if (player.Playing)
                {
                    player.Stop();
                }
            }

            _sfxPlayer2D.Stop();

            _musicTracks.Clear();
            _soundEffects.Clear();

            GD.Print("[AudioManager] 音频资源清理完成");
        }

        public override void _ExitTree()
        {
            Cleanup();
            base._ExitTree();
        }
    }

    /// <summary>
    /// 音频设置数据结构
    /// </summary>
    public class AudioSettings
    {
        public float MasterVolume { get; set; } = 1.0f;
        public float MusicVolume { get; set; } = 0.7f;
        public float SfxVolume { get; set; } = 0.8f;
        public bool IsMusicMuted { get; set; } = false;
        public bool IsSfxMuted { get; set; } = false;
    }
}
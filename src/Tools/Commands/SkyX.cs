using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia.Tools.Commands
{
	class SkyX : Command
	{
		#region Fields
		MSkyX.MSkyX _sky = null;
		#endregion Fields

		#region Commands
		private void OnAtmosphere(ArgEvent e)
		{
			if (e.subArgs.Count < 1)
			{
				p.EmptyParamList(e.name);
				return;
			}
			MSkyX.AtmosphereManager.Options o = _sky.AtmosphereMgr.CurrentOptions;
			foreach (KeyValuePair<string, List<string>> pair in e.subArgs)
			{
				if (pair.Value.Count == 0)
				{
					p.EmptyParamList(e.name + " " + pair.Key);
					continue;
				}
				int? prevI;
				float? prevF;
				Mogre.Vector3? prevV3;
				switch (pair.Key)
				{
					case "exposure":
						{
							prevF = ParseFloat(pair.Value[0], ref o.Exposure);
							if (prevF != null)
								p.ParamChanged("Atmosphere exposure", prevF.ToString(), o.Exposure.ToString());
							else
								p.UnrecognisedParam(e.name, pair.Key);
							break;
						}
					case "phase":
						{
							prevF = ParseFloat(pair.Value[0], ref o.G);
							if (prevF != null)
								p.ParamChanged("Atmosphere phase", prevF.ToString(), o.G.ToString());
							else
								p.UnrecognisedParam(e.name, pair.Key);
							break;
						}
					case "heightposition":
						{
							prevF = ParseFloat(pair.Value[0], ref o.HeightPosition);
							if (prevF != null)
								p.ParamChanged("Atmosphere height position", prevF.ToString(), o.HeightPosition.ToString());
							else
								p.UnrecognisedParam(e.name, pair.Key);
							break;
						}
					case "innerradius":
						{
							prevF = ParseFloat(pair.Value[0], ref o.InnerRadius);
							if (prevF != null)
								p.ParamChanged("Atmosphere inner radius", prevF.ToString(), o.InnerRadius.ToString());
							else
								p.UnrecognisedParam(e.name, pair.Key);
							break;
						}
					case "outerradius":
						{
							prevF = ParseFloat(pair.Value[0], ref o.OuterRadius);
							if (prevF != null)
								p.ParamChanged("Atmosphere outer radius", prevF.ToString(), o.OuterRadius.ToString());
							else
								p.UnrecognisedParam(e.name, pair.Key);
							break;
						}
					case "miemultiplier":
						{
							prevF = ParseFloat(pair.Value[0], ref o.MieMultiplier);
							if (prevF != null)
								p.ParamChanged("Atmosphere mie multiplier", prevF.ToString(), o.MieMultiplier.ToString());
							else
								p.UnrecognisedParam(e.name, pair.Key);
							break;
						}
					case "rayleighmultiplier":
						{
							prevF = ParseFloat(pair.Value[0], ref o.RayleighMultiplier);
							if (prevF != null)
								p.ParamChanged("Atmosphere rayleigh multiplier", prevF.ToString(), o.RayleighMultiplier.ToString());
							else
								p.UnrecognisedParam(e.name, pair.Key);
							break;
						}
					case "sunintensity":
						{
							prevF = ParseFloat(pair.Value[0], ref o.SunIntensity);
							if (prevF != null)
								p.ParamChanged("Atmosphere sun intensity", prevF.ToString(), o.SunIntensity.ToString());
							else
								p.UnrecognisedParam(e.name, pair.Key);
							break;
						}
					case "wavelength":
						{
							prevV3 = ParseVector3(pair.Value, ref o.WaveLength);
							if (prevV3 != null)
								p.ParamChanged("Atmosphere wave length", prevV3.ToString(), o.WaveLength.ToString());
							else
								p.UnrecognisedParam(e.name, pair.Key);
							break;
						}
					case "numberofsamles":
						{
							prevI = ParseInt(pair.Value[0], ref o.NumberOfSamples);
							if (prevI != null)
								p.ParamChanged("Atmosphere number of samles", prevI.ToString(), o.NumberOfSamples.ToString());
							else
								p.UnrecognisedParam(e.name, pair.Key);
							break;
						}
					default:
						{
							p.UnrecognisedSubArg(e.name, pair.Key);
							break;
						}
				}
			}
			_sky.AtmosphereMgr.CurrentOptions = o;
		}
		private void OnMode(ArgEvent e)
		{
			if (e.parameters == null || e.parameters.Count < 1)
			{
				p.EmptyParamList(e.name);
				return;
			}
			MSkyX.MSkyX.LightingMode prev = _sky.CurrentLightingMode;
			switch (e.parameters[0])
			{
				case "hdr":
					_sky.CurrentLightingMode = MSkyX.MSkyX.LightingMode.LM_HDR;
					p.ParamChanged("LightningMode", prev.ToString(), _sky.CurrentLightingMode.ToString());
					break;
				case "ldr":
					_sky.CurrentLightingMode = MSkyX.MSkyX.LightingMode.LM_LDR;
					p.ParamChanged("LightningMode", prev.ToString(), _sky.CurrentLightingMode.ToString());
					break;
				default:
					p.UnrecognisedParam(e.name);
					break;
			}
		}
		private void OnMoon(ArgEvent e)
		{
			if (e.subArgs.Count < 1)
			{
				p.EmptyParamList(e.name);
				return;
			}

			float? prev;
			float val;
			foreach (KeyValuePair<string, List<string>> pair in e.subArgs)
			{
				switch (pair.Key)
				{
					case "size":
						{
							val = _sky.MoonMgr.MoonSize;
							if ((prev = ParseFloat(pair.Value[0], ref val)) != null)
							{
								_sky.MoonMgr.MoonSize = val;
								p.ParamChanged("Moon size", prev.Value.ToString(), val.ToString());
							}
							else
								p.UnrecognisedParam(e.name, pair.Key);
							break;
						}
					case "phase":
						{
							val = 0;
							if ((prev = ParseFloat(pair.Value[0], ref val)) != null)
							{
								_sky.MoonMgr.UpdateMoonPhase(val);
								p.ParamChanged("Moon phase", null, val.ToString());
							}
							else
								p.UnrecognisedParam(e.name, pair.Key);
							break;
						}
					case "halointensity":
						{
							val = _sky.MoonMgr.MoonHaloIntensity;
							if ((prev = ParseFloat(pair.Value[0], ref val)) != null)
							{
								_sky.MoonMgr.MoonHaloIntensity = val;
								p.ParamChanged("Moon halo intensity", prev.Value.ToString(), val.ToString());
							}
							else
								p.UnrecognisedParam(e.name, pair.Key);
							break;
						}
					case "halostrength":
						{
							val = _sky.MoonMgr.MoonHaloStrength;
							if ((prev = ParseFloat(pair.Value[0], ref val)) != null)
							{
								_sky.MoonMgr.MoonHaloStrength = val;
								p.ParamChanged("Moon halo strenght", prev.Value.ToString(), val.ToString());
							}
							else
								p.UnrecognisedParam(e.name, pair.Key);
							break;
						}
					default:
						{
							p.UnrecognisedSubArg(e.name, pair.Key);
							break;
						}
				}
			}
		}
		private void OnStarfield(ArgEvent e)
		{
			if (e.parameters.Count < 1)
			{
				p.EmptyParamList(e.name);
				return;
			}

			bool val = _sky.StarfieldEnabled;
			bool? prev;

			prev = ParseBool(e.parameters[0], ref val);
			if (prev != null)
			{
				_sky.StarfieldEnabled = val;
				p.ParamChanged("Starfield", prev.Value.ToString(), val.ToString());
			}
			else
				p.UnrecognisedParam(e.name);
		}
		private void OnTime(ArgEvent e)
		{
			if (e.parameters.Count < 1)
			{
				p.EmptyParamList(e.name);
				return;
			}

			float val = _sky.TimeMultiplier;
			if (ParseFloat(e.parameters[0], ref val) != null)
			{
				p.ParamChanged("Time multiplier", _sky.TimeMultiplier.ToString(), val.ToString());
				_sky.TimeMultiplier = val;
			}
			else
				p.UnrecognisedParam(e.name);
		}
		private void OnVisible(ArgEvent e)
		{
			if (e.parameters.Count < 1)
			{
				p.EmptyParamList(e.name);
				return;
			}

			bool val = _sky.Visible;
			bool? prev;

			prev = ParseBool(e.parameters[0], ref val);
			if (prev!= null)
			{
				_sky.Visible = val;
				p.ParamChanged("Starfield", prev.Value.ToString(), val.ToString());
			}
			else
				p.UnrecognisedParam(e.name);
		}
		#endregion Commands

		#region ICommand
		public override void OnInvoke(CommandEvent e)
		{
			_sky = Core.Singleton.SkyX;
			base.OnInvoke(e);
		}
		#endregion Icommand

		#region Methods
		public SkyX()
			: base("skyx")
		{
		}
		protected override void RegisterArgs()
		{
			#region Atmosphere Manager

			RegisterArg("atmosphere", OnAtmosphere);
			RegisterAlias("atmosphere", "a");
			RegisterSubArg("atmosphere", "exposure");
			RegisterSubArg("atmosphere", "phase");
			RegisterSubArg("atmosphere", "heightposition");
			RegisterSubArg("atmosphere", "innerradius");
			RegisterSubArg("atmosphere", "outerradius");
			RegisterSubArg("atmosphere", "miemultiplier");
			RegisterSubArg("atmosphere", "rayleighmultiplier");
			RegisterSubArg("atmosphere", "sunintensity");
			RegisterSubArg("atmosphere", "wavelength");
			RegisterSubArg("atmosphere", "numberofsamles");

			#endregion Atmosphere Manager

			#region Clouds Manager

			RegisterArg("clouds", Dummy);
			RegisterAlias("clouds", "c");

			#endregion Clouds Manager
			
			#region General

			RegisterArg("mode", OnMode);
			RegisterArg("starfield", OnStarfield);
			RegisterAlias("starfield", "s");
			RegisterArg("timemultiplier", OnTime);
			RegisterAlias("timemultiplier", "time");
			RegisterAlias("timemultiplier", "t");
			RegisterArg("visible", OnVisible);
			RegisterAlias("visible", "v");

			#endregion General
			
			#region Moon Manager

			RegisterArg("moon", OnMoon);
			RegisterAlias("moon", "m");
			RegisterSubArg("moon", "size");
			RegisterSubArg("moon", "phase");
			RegisterSubArg("moon", "halointensity");
			RegisterSubArg("moon", "halostrength");
			
			#endregion Moon Manager
			
			#region VClouds Manager

			RegisterArg("vclouds", Dummy);
			RegisterAlias("vclouds", "vc");

			#endregion VClouds Manager
		}
		#endregion Methods
	}
}

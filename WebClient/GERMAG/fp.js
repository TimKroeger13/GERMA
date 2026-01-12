// User Fingerprinting System
class UserFingerprint {
  constructor() {
    this.fingerprintData = {};
    this.userId = null;
  }

  // Generate a hash from string
  async hashString(str) {
    const encoder = new TextEncoder();
    const data = encoder.encode(str);
    const hashBuffer = await crypto.subtle.digest('SHA-256', data);
    const hashArray = Array.from(new Uint8Array(hashBuffer));
    return hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
  }

  // Get screen fingerprint
  getScreenFingerprint() {
    return {
      width: screen.width,
      height: screen.height,
      colorDepth: screen.colorDepth,
      pixelDepth: screen.pixelDepth,
      availWidth: screen.availWidth,
      availHeight: screen.availHeight
    };
  }

  // Get canvas fingerprint
  getCanvasFingerprint() {
    try {
      const canvas = document.createElement('canvas');
      const ctx = canvas.getContext('2d');
      
      // Draw some text and shapes
      ctx.textBaseline = 'top';
      ctx.font = '14px Arial';
      ctx.fillText('Browser fingerprinting 🎯', 2, 2);
      
      // Draw some shapes
      ctx.fillStyle = 'rgba(102, 204, 0, 0.7)';
      ctx.fillRect(100, 5, 80, 20);
      
      return canvas.toDataURL();
    } catch (e) {
      return 'canvas-error';
    }
  }

  // Get WebGL fingerprint
  getWebGLFingerprint() {
    try {
      const canvas = document.createElement('canvas');
      const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');
      
      if (!gl) return 'no-webgl';
      
      const renderer = gl.getParameter(gl.RENDERER);
      const vendor = gl.getParameter(gl.VENDOR);
      const version = gl.getParameter(gl.VERSION);
      const shadingLanguageVersion = gl.getParameter(gl.SHADING_LANGUAGE_VERSION);
      
      return `${renderer}|${vendor}|${version}|${shadingLanguageVersion}`;
    } catch (e) {
      return 'webgl-error';
    }
  }

  // Get audio context fingerprint
  getAudioFingerprint() {
    return new Promise((resolve) => {
      try {
        const audioContext = new (window.AudioContext || window.webkitAudioContext)();
        const oscillator = audioContext.createOscillator();
        const analyser = audioContext.createAnalyser();
        const gainNode = audioContext.createGain();
        const scriptProcessor = audioContext.createScriptProcessor(4096, 1, 1);

        oscillator.type = 'triangle';
        oscillator.frequency.setValueAtTime(10000, audioContext.currentTime);

        gainNode.gain.setValueAtTime(0, audioContext.currentTime);

        oscillator.connect(analyser);
        analyser.connect(scriptProcessor);
        scriptProcessor.connect(gainNode);
        gainNode.connect(audioContext.destination);

        scriptProcessor.onaudioprocess = function(bins) {
          const samples = bins.inputBuffer.getChannelData(0);
          let sum = 0;
          for (let i = 0; i < samples.length; i++) {
            sum += Math.abs(samples[i]);
          }
          
          oscillator.disconnect();
          scriptProcessor.disconnect();
          audioContext.close();
          
          resolve(sum.toString());
        };

        oscillator.start(0);
      } catch (e) {
        resolve('audio-error');
      }
    });
  }

  // Get timezone and language info
  getLocaleFingerprint() {
    return {
      timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
      language: navigator.language,
      languages: navigator.languages ? navigator.languages.join(',') : '',
      platform: navigator.platform,
      userAgent: navigator.userAgent,
      cookieEnabled: navigator.cookieEnabled,
      doNotTrack: navigator.doNotTrack
    };
  }

  // Get font fingerprint
  getFontFingerprint() {
    const fonts = [
      'Arial', 'Helvetica', 'Times', 'Times New Roman', 'Courier', 'Courier New',
      'Verdana', 'Georgia', 'Palatino', 'Garamond', 'Bookman', 'Comic Sans MS',
      'Trebuchet MS', 'Arial Black', 'Impact', 'Calibri', 'Cambria', 'Tahoma'
    ];
    
    const testString = 'mmmmmmmmmmlli';
    const testSize = '72px';
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    
    // Baseline with monospace
    ctx.font = `${testSize} monospace`;
    const baseline = ctx.measureText(testString).width;
    
    const availableFonts = [];
    
    fonts.forEach(font => {
      ctx.font = `${testSize} ${font}, monospace`;
      const width = ctx.measureText(testString).width;
      if (width !== baseline) {
        availableFonts.push(font);
      }
    });
    
    return availableFonts.join(',');
  }

  // Collect all fingerprint data
  async collectFingerprint() {
    this.fingerprintData = {
      screen: this.getScreenFingerprint(),
      canvas: this.getCanvasFingerprint(),
      webgl: this.getWebGLFingerprint(),
      audio: await this.getAudioFingerprint(),
      locale: this.getLocaleFingerprint(),
      fonts: this.getFontFingerprint(),
      timestamp: Date.now()
    };
    
    // Create fingerprint string
    const fingerprintString = JSON.stringify(this.fingerprintData);
    const fingerprintHash = await this.hashString(fingerprintString);
    
    return fingerprintHash;
  }

  // Get or generate user ID
  async getUserId() {
    // First, try to get existing ID from localStorage
    const existingId = localStorage.getItem('anonymous_user_id');
    
    if (existingId) {
      this.userId = existingId;
      return existingId;
    }
    
    // Generate fingerprint-based ID
    const fingerprint = await this.collectFingerprint();
    
    // Add some randomness to make it harder to correlate
    const randomComponent = Math.random().toString(36).substring(2, 15);
    const combinedId = await this.hashString(`${fingerprint}_${randomComponent}`);
    
    // Store in localStorage for persistence
    localStorage.setItem('anonymous_user_id', combinedId);
    
    this.userId = combinedId;
    return combinedId;
  }

  // Send fingerprint to API
  async sendToAPI(apiEndpoint) {
    const userId = await this.getUserId();
    
    try {
      const response = await fetch(apiEndpoint, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          userId: userId,
          fingerprint: this.fingerprintData,
          timestamp: Date.now()
        })
      });
      
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      
      const result = await response.json();
      return result;
    } catch (error) {
      console.error('Failed to send fingerprint to API:', error);
      throw error;
    }
  }
}

// Usage example
async function initializeUserTracking() {
  const fingerprinter = new UserFingerprint();
  
  try {
    // Get anonymous user ID
    const userId = await fingerprinter.getUserId();
    
    return userId;
  } catch (error) {
    console.error('Error initializing user tracking:', error);
  }
}

// Initialize when page loads
document.addEventListener('DOMContentLoaded', initializeUserTracking);

// Export for use in modules
if (typeof module !== 'undefined' && module.exports) {
  module.exports = UserFingerprint;
}
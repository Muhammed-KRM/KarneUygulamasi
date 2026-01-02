import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminSigninModalComponent } from './admin-signin-modal/admin-signin-modal.component';
import { UserManagementComponent } from './user-management/user-management.component';
import { UserManagementComponentUpdateModal } from './user-management/user-management.component-update-modal';
import { LogsComponent } from './logs/logs.component';

@Component({
  selector: 'app-tools',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    AdminSigninModalComponent, 
    UserManagementComponent,
    UserManagementComponentUpdateModal,
    LogsComponent
  ],
  templateUrl: './tools.component.html',
  styleUrl: './tools.component.scss'
})
export class ToolsComponent {
  activeModal: string | null = null;
  activeUtilityTool: string | null = null;

  // Color Picker
  selectedColor = '#ff6b9d';
  hexColor = '#ff6b9d';
  rgbColor = 'rgb(255, 107, 157)';
  hslColor = 'hsl(342, 100%, 71%)';

  // Text Utils
  textInput = '';
  textStats = {
    characters: 0,
    words: 0,
    lines: 0
  };

  // Password Generator
  generatedPassword = '';
  passwordLength = 16;
  passwordOptions = {
    uppercase: true,
    lowercase: true,
    numbers: true,
    symbols: true
  };

  // Base64
  base64Input = '';
  base64Output = '';
  base64Mode: 'encode' | 'decode' = 'encode';

  // JSON Formatter
  jsonInput = '';
  jsonOutput = '';
  jsonError = '';

  // URL Encoder/Decoder
  urlInput = '';
  urlOutput = '';
  urlMode: 'encode' | 'decode' = 'encode';

  openModal(modalName: string) {
    this.activeModal = modalName;
    document.body.style.overflow = 'hidden';
  }

  closeModal() {
    this.activeModal = null;
    document.body.style.overflow = '';
  }

  openUtilityTool(toolName: string) {
    this.activeUtilityTool = toolName;
    document.body.style.overflow = 'hidden';
    
    // Initialize tool-specific data
    if (toolName === 'passwordGen' && !this.generatedPassword) {
      this.generatePassword();
    }
  }

  closeUtilityTool() {
    this.activeUtilityTool = null;
    document.body.style.overflow = '';
  }

  // Color Picker Methods
  updateColorFormats() {
    this.hexColor = this.selectedColor.toUpperCase();
    const r = parseInt(this.selectedColor.slice(1, 3), 16);
    const g = parseInt(this.selectedColor.slice(3, 5), 16);
    const b = parseInt(this.selectedColor.slice(5, 7), 16);
    
    this.rgbColor = `rgb(${r}, ${g}, ${b})`;
    
    const hsl = this.rgbToHsl(r, g, b);
    this.hslColor = `hsl(${hsl.h}, ${hsl.s}%, ${hsl.l}%)`;
  }

  updateFromHex() {
    if (/^#[0-9A-F]{6}$/i.test(this.hexColor)) {
      this.selectedColor = this.hexColor;
      this.updateColorFormats();
    }
  }

  rgbToHsl(r: number, g: number, b: number) {
    r /= 255;
    g /= 255;
    b /= 255;
    const max = Math.max(r, g, b);
    const min = Math.min(r, g, b);
    let h = 0, s = 0;
    const l = (max + min) / 2;

    if (max !== min) {
      const d = max - min;
      s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
      switch (max) {
        case r: h = ((g - b) / d + (g < b ? 6 : 0)) / 6; break;
        case g: h = ((b - r) / d + 2) / 6; break;
        case b: h = ((r - g) / d + 4) / 6; break;
      }
    }
    return {
      h: Math.round(h * 360),
      s: Math.round(s * 100),
      l: Math.round(l * 100)
    };
  }

  // Text Utils Methods
  updateTextStats() {
    this.textStats.characters = this.textInput.length;
    this.textStats.words = this.textInput.trim() ? this.textInput.trim().split(/\s+/).length : 0;
    this.textStats.lines = this.textInput.split('\n').length;
  }

  transformText(type: string) {
    switch (type) {
      case 'uppercase':
        this.textInput = this.textInput.toUpperCase();
        break;
      case 'lowercase':
        this.textInput = this.textInput.toLowerCase();
        break;
      case 'title':
        this.textInput = this.textInput.replace(/\w\S*/g, (txt) => 
          txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase()
        );
        break;
      case 'sentence':
        this.textInput = this.textInput.charAt(0).toUpperCase() + 
          this.textInput.slice(1).toLowerCase();
        break;
    }
    this.updateTextStats();
  }

  async copyToClipboard() {
    try {
      await navigator.clipboard.writeText(this.textInput);
      alert('Metin kopyalandı!');
    } catch (err) {
      console.error('Kopyalama hatası:', err);
    }
  }

  clearText() {
    this.textInput = '';
    this.updateTextStats();
  }

  // Password Generator Methods
  generatePassword() {
    let charset = '';
    if (this.passwordOptions.uppercase) charset += 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    if (this.passwordOptions.lowercase) charset += 'abcdefghijklmnopqrstuvwxyz';
    if (this.passwordOptions.numbers) charset += '0123456789';
    if (this.passwordOptions.symbols) charset += '!@#$%^&*()_+-=[]{}|;:,.<>?';

    if (!charset) {
      alert('En az bir seçenek seçmelisiniz!');
      return;
    }

    let password = '';
    for (let i = 0; i < this.passwordLength; i++) {
      password += charset.charAt(Math.floor(Math.random() * charset.length));
    }
    this.generatedPassword = password;
  }

  async copyPassword() {
    try {
      await navigator.clipboard.writeText(this.generatedPassword);
      alert('Şifre kopyalandı!');
    } catch (err) {
      console.error('Kopyalama hatası:', err);
    }
  }

  // Base64 Methods
  processBase64() {
    try {
      if (this.base64Mode === 'encode') {
        this.base64Output = btoa(unescape(encodeURIComponent(this.base64Input)));
      } else {
        this.base64Output = decodeURIComponent(escape(atob(this.base64Input)));
      }
    } catch (error) {
      this.base64Output = 'Hata: Geçersiz veri!';
    }
  }

  async copyBase64() {
    try {
      await navigator.clipboard.writeText(this.base64Output);
      alert('Kopyalandı!');
    } catch (err) {
      console.error('Kopyalama hatası:', err);
    }
  }

  clearBase64() {
    this.base64Input = '';
    this.base64Output = '';
  }

  // JSON Formatter Methods
  formatJSON() {
    try {
      const parsed = JSON.parse(this.jsonInput);
      this.jsonOutput = JSON.stringify(parsed, null, 2);
      this.jsonError = '';
    } catch (error: any) {
      this.jsonError = 'Hata: ' + error.message;
      this.jsonOutput = '';
    }
  }

  minifyJSON() {
    try {
      const parsed = JSON.parse(this.jsonInput);
      this.jsonOutput = JSON.stringify(parsed);
      this.jsonError = '';
    } catch (error: any) {
      this.jsonError = 'Hata: ' + error.message;
      this.jsonOutput = '';
    }
  }

  async copyJSON() {
    try {
      await navigator.clipboard.writeText(this.jsonOutput);
      alert('Kopyalandı!');
    } catch (err) {
      console.error('Kopyalama hatası:', err);
    }
  }

  clearJSON() {
    this.jsonInput = '';
    this.jsonOutput = '';
    this.jsonError = '';
  }

  // URL Encoder/Decoder Methods
  processURL() {
    try {
      if (this.urlMode === 'encode') {
        this.urlOutput = encodeURIComponent(this.urlInput);
      } else {
        this.urlOutput = decodeURIComponent(this.urlInput);
      }
    } catch (error) {
      this.urlOutput = 'Hata: Geçersiz URL!';
    }
  }

  async copyURL() {
    try {
      await navigator.clipboard.writeText(this.urlOutput);
      alert('Kopyalandı!');
    } catch (err) {
      console.error('Kopyalama hatası:', err);
    }
  }

  clearURL() {
    this.urlInput = '';
    this.urlOutput = '';
  }
}

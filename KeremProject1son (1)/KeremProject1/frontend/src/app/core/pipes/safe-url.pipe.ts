import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

@Pipe({
  name: 'safeUrl',
  standalone: true
})
export class SafeUrlPipe implements PipeTransform {
  constructor(private sanitizer: DomSanitizer) {}

  transform(value: string | null | undefined): SafeUrl {
    if (!value) return this.sanitizer.bypassSecurityTrustUrl('');
    return this.sanitizer.bypassSecurityTrustUrl(value);
  }
}


import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideZoneChangeDetection,
} from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { loadingInterceptor } from './core/interceptors/loading.interceptor';
import { InitService } from './core/services/init.service';
import { defer, finalize, lastValueFrom } from 'rxjs';

// async function initializeApp() {
//   try {
//     const initService = inject(InitService);
//     return await lastValueFrom(initService.init());
//   } finally {
//     const splash = document.getElementById('initial-splash');
//     if (splash) {
//       splash.remove();
//     }
//   }
// }

function initializeApp() {
  const initService = inject(InitService);
  return defer(() => initService.init()).pipe(
    finalize(() => {
      const splash = document.getElementById('initial-splash');
      if (splash) {
        splash.remove();
      }
    })
  );
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([errorInterceptor, loadingInterceptor])),
    provideAppInitializer(initializeApp),
  ],
};

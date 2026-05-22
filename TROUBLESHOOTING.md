# Troubleshooting Guide

## Common Issues and Solutions

### Issue 1: Login API Returns 404
**Problem**: `/api/accountapi/login` endpoint returns 404

**Solutions**:
1. Ensure `AddControllersWithViews()` is called in Program.cs ✓ (Already configured)
2. Verify AccountApiController.cs exists in Controllers folder ✓
3. Check controller namespace matches: `namespace NewsSite.Controllers`
4. Verify `[ApiController]` and `[Route("api/[controller]")]` attributes are present
5. Rebuild solution after adding new controller
6. Clear browser cache (Ctrl+F5)

### Issue 2: Login Button Doesn't Open Offcanvas
**Problem**: Login button in navbar doesn't trigger offcanvas

**Solutions**:
1. Verify Bootstrap bundle is loaded: Check `<script src="~/lib/bootstrap/js/bootstrap.bundle.min.js">`
2. Ensure offcanvas div exists in _Layout.cshtml with id="loginOffcanvas"
3. Check button has correct attributes:
   - `data-bs-toggle="offcanvas"`
   - `data-bs-target="#loginOffcanvas"`
4. Verify no JavaScript errors in browser console (F12)
5. Check Bootstrap version supports offcanvas (5.0+)

### Issue 3: Banner Doesn't Appear
**Problem**: Subscribe banner never shows on home page

**Solutions**:
1. Verify user is NOT authenticated (check if User.Identity.IsAuthenticated is false)
2. Verify user does NOT have active subscription (check Model.HasActiveSubscription)
3. Check browser console for JavaScript errors
4. Verify localStorage is enabled in browser
5. If banner was dismissed previously:
   - Clear localStorage: `localStorage.removeItem('subscribeBannerDismissed')`
   - Or reload page in private/incognito mode
6. Verify timeout is set correctly (12000ms = 12 seconds)
7. Check z-index conflict: banner uses `z-index: 999`

### Issue 4: AJAX Login Doesn't Work, Page Reloads
**Problem**: Form submits traditional way instead of AJAX

**Solutions**:
1. Verify button type is `type="button"` NOT `type="submit"`
2. Check `onclick="submitLoginForm()"` is present on button
3. Verify form ID is `id="loginOffcanvasForm"`
4. Check JavaScript console for errors in submitLoginForm()
5. Ensure fetch API is available (not IE11 - use polyfill if needed)
6. Verify Content-Type header is set to 'application/json'

### Issue 5: Offcanvas Doesn't Close After Login
**Problem**: Offcanvas stays open after successful login

**Solutions**:
1. Add null check: `if (loginOffcanvas) { loginOffcanvas.hide(); }`
2. Verify Bootstrap Offcanvas is initialized correctly
3. Check page reload happens after hide (1000ms delay)
4. If reload is too fast, increase delay: `setTimeout(() => { location.reload(); }, 1500);`
5. Verify offcanvas ID matches: `id="loginOffcanvas"`

### Issue 6: Navbar Doesn't Update After Login
**Problem**: After login, navbar still shows Login/Register buttons

**Solutions**:
1. This is expected - page reload required (happens automatically)
2. If reload doesn't work, check browser console for errors
3. Verify `location.reload()` is called in success handler
4. Check server-side session/cookie is properly set
5. Verify user is actually signed in (check HttpContext.User.Identity.IsAuthenticated)

### Issue 7: Login Success but Then Shows Error
**Problem**: Successful login immediately shows error alert

**Solutions**:
1. Check API response status code - should be 200
2. Verify `response.ok` check is working correctly
3. Check if `data.json()` is being called correctly
4. Add console.log to debug: `console.log('Status:', response.status); console.log('Data:', data);`
5. Verify Content-Type in response is 'application/json'

### Issue 8: Banner Text Shows in Swedish But Shouldn't
**Problem**: Banner shows Swedish text regardless of locale

**Solutions**:
1. This is by design - project uses Swedish
2. To change language, edit text in Home/Index.cshtml
3. Search for "Prenumerera" and replace with English equivalent
4. Update all banner text strings

### Issue 9: Banner Appears Too Frequently
**Problem**: Banner shows again after dismissal

**Solutions**:
1. Check localStorage is enabled in browser
2. Verify dismissal function is called: `dismissBanner()`
3. Add to _Layout if not present: `<button onclick="dismissBanner()">Close</button>`
4. Check for multiple banner instances (ID should be unique)
5. Verify localStorage key is exactly: `subscribeBannerDismissed`

### Issue 10: CORS Error When Calling API
**Problem**: Cross-Origin Request Blocked error

**Solutions**:
1. This shouldn't occur since API is same-origin
2. If using different subdomain/port:
   - Add CORS policy in Program.cs
   - Add `[EnableCors("PolicyName")]` to controller
3. Verify URL is relative: `/api/accountapi/login` (not absolute)
4. Check browser console for exact CORS error message

## Debug Techniques

### Enable Console Logging
Add to submitLoginForm() for debugging:
```javascript
console.log('Email:', email);
console.log('Password:', password);
console.log('RememberMe:', rememberMe);
console.log('Fetching:', '/api/accountapi/login');
```

### Check API Response
Add after fetch:
```javascript
console.log('Response Status:', response.status);
console.log('Response OK:', response.ok);
console.log('Response Data:', data);
```

### Monitor LocalStorage
In browser console:
```javascript
localStorage.getItem('subscribeBannerDismissed')  // Check if set
localStorage.removeItem('subscribeBannerDismissed')  // Clear
localStorage.clear()  // Clear all
```

### Check Form Values Before Submit
Add to submitLoginForm():
```javascript
console.log('Form data:', { email, password, rememberMe });
```

## Testing Credentials

If not already set up, use these test accounts:
- **Email**: anna.bergman@newsite.com
- **Password**: Writer123!
- **Role**: Writer

Or use admin credentials from seed data:
- **Email**: admin@newssite.com
- **Password**: Admin123!
- **Role**: Admin

## Browser DevTools Shortcuts

- **F12**: Open DevTools
- **Ctrl+Shift+J**: Open Console
- **Ctrl+Shift+I**: Open Inspector
- **Ctrl+Shift+K**: Open Console (Firefox)
- **Ctrl+F5**: Hard refresh (clear cache)
- **Ctrl+Shift+Delete**: Clear Cache/Storage

## Performance Considerations

1. **Banner Delay**: 12 seconds is reasonable, increase if users click too fast
2. **Scroll Threshold**: 60% is good balance between visibility and UX
3. **Page Reload**: Necessary for navbar update, consider using AJAX if available
4. **API Response Time**: Monitor `/api/accountapi/login` response time
5. **LocalStorage**: Uses minimal space (only one small string per dismissal)

## Security Notes

1. **HTTPS Required**: Ensure login form only works over HTTPS
2. **CSRF Protection**: Consider adding CSRF token if not using cookies
3. **Rate Limiting**: Implement on API to prevent brute force
4. **Password Input**: Already masked with `type="password"`
5. **Remember Me**: Server-side implementation is secure

## Browser Support

- ✅ Chrome 90+
- ✅ Firefox 88+
- ✅ Safari 14+
- ✅ Edge 90+
- ❌ Internet Explorer 11 (fetch API not supported)
- ⚠️ Mobile browsers (tested on iOS Safari, Chrome Mobile)

## Known Limitations

1. Two-factor authentication returns error instead of prompting
2. External login providers not supported in AJAX flow
3. Offcanvas doesn't support modal behavior (can still click outside)
4. Banner only checks on page load and scroll (not on dynamic content)
5. Subscribe form still uses traditional POST (not AJAX)

## Contact & Support

For issues not covered here:
1. Check browser console for error messages
2. Review Network tab in DevTools for failed requests
3. Check server logs for exceptions
4. Verify all files are properly saved
5. Try clearing browser cache and rebuilding solution


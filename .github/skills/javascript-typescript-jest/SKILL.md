---
name: javascript-typescript-jest
description: 'Best practices for writing JavaScript/TypeScript tests using Jest, including mocking strategies, test structure, and common patterns.'
---

# JavaScript/TypeScript Jest Best Practices

Your goal is to help write effective tests for JavaScript/TypeScript code using Jest, following best practices for test structure, mocking, and async code.

## Test Structure

- Name test files with `.test.ts` or `.test.js` suffix
- Place test files next to the code they test or in a dedicated `__tests__` directory
- Use descriptive test names that explain the expected behavior
- Use nested `describe` blocks to organize related tests
- Follow the pattern:
  ```ts
  describe('Component/Function/Class', () => {
    it('should do something when condition', () => { /* ... */ })
  })
  ```

## Effective Mocking

- Mock external dependencies (APIs, network calls, etc.) to isolate your tests
- Use `jest.mock()` for module-level mocks
- Use `jest.spyOn()` for specific function mocks
- Use `mockImplementation()` or `mockReturnValue()` to define mock behavior
- Reset mocks between tests with `jest.resetAllMocks()` in `afterEach`

## Testing Async Code

- Always return promises or use `async`/`await` syntax in tests
- Use `resolves`/`rejects` matchers for promises:
  ```ts
  await expect(fetchUser(1)).resolves.toEqual({ id: 1, name: 'Alice' });
  await expect(fetchUser(-1)).rejects.toThrow('Not found');
  ```
- Set appropriate timeouts for slow tests with `jest.setTimeout()`

## Snapshot Testing

- Use snapshot tests for UI components or complex objects that change infrequently
- Keep snapshots small and focused
- Review snapshot changes carefully before committing

## Testing React Native Components

- Use React Native Testing Library (`@testing-library/react-native`) for testing components
- Test user behavior and component accessibility
- Query elements by accessibility roles, labels, or text content
- Use `userEvent` over `fireEvent` for more realistic user interactions

## Common Jest Matchers

```ts
// Equality
expect(value).toBe(expected);          // Strict equality (===)
expect(value).toEqual(expected);       // Deep equality

// Truthiness
expect(value).toBeTruthy();
expect(value).toBeFalsy();
expect(value).toBeNull();
expect(value).toBeUndefined();

// Numbers
expect(value).toBeGreaterThan(3);
expect(value).toBeLessThanOrEqual(10);

// Strings
expect(value).toMatch(/pattern/);
expect(value).toContain('substring');

// Arrays
expect(array).toContain(item);
expect(array).toHaveLength(3);

// Objects
expect(object).toHaveProperty('key', value);
expect(object).toMatchObject({ partial: 'match' });

// Exceptions
expect(fn).toThrow();
expect(fn).toThrow(Error);

// Mock functions
expect(mockFn).toHaveBeenCalled();
expect(mockFn).toHaveBeenCalledWith(arg1, arg2);
expect(mockFn).toHaveBeenCalledTimes(2);
```

## Zustand Store Testing

```ts
// Test Zustand stores by acting on the store and asserting state
import { renderHook, act } from '@testing-library/react-native';
import { useAuthStore } from '../stores/authStore';

beforeEach(() => {
  useAuthStore.setState({ user: null, token: null });
});

it('should set user on login', () => {
  act(() => {
    useAuthStore.getState().setUser({ id: '1', name: 'Alice' });
  });
  expect(useAuthStore.getState().user).toEqual({ id: '1', name: 'Alice' });
});
```

## API Service Testing

```ts
// Mock the apiClient module
jest.mock('../services/apiClient');
import { apiClient } from '../services/apiClient';

it('should call the correct endpoint', async () => {
  (apiClient.get as jest.Mock).mockResolvedValue({ data: { matches: [] } });
  const result = await getMatches();
  expect(apiClient.get).toHaveBeenCalledWith('/matches');
  expect(result).toEqual([]);
});
```

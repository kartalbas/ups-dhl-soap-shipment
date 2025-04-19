# Shipping Label Management System

A comprehensive solution for managing shipping labels across multiple carriers (UPS and DHL).

## System Components

### ShipmentLib
A shared library providing common functionality used across all system components.

### ShipmentModule
The primary executable for managing shipping labels, offering the following features:
- Create shipping labels for UPS and DHL
- Revoke existing labels
- Track shipments

### ShipmentClient
A utility tool for:
- Manual revocation of shipping labels
- Printing shipping reports

## Carrier Implementations
- **ShipmentDHL**: Original DHL implementation.
- **ShipmentDHL3**: Enhanced DHL implementation with improved features.
- **ShipmentUPS**: UPS implementation.

## Testing
- **ShipmentTest**: A dedicated test suite for DHL endpoints.

## License

This project is licensed under the **Business Source License 1.1 (BSL 1.1)**. Under this license:

- The source code is available for personal and non-commercial use.
- Commercial use by companies requires a separate commercial license.
- After a specified change date, the code will be made available under the Apache 2.0 License.

For more details, refer to the [LICENSE](LICENSE) file.

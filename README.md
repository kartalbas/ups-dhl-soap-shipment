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

This project is licensed under the Business Source License 1.1 (BSL 1.1).  
See the [LICENSE](./LICENSE) file for details.